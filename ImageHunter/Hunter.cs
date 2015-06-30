using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using ImageHunter.FileProvider;
using ImageHunter.Logging;
using ImageHunter.ShortUrls;

namespace ImageHunter
{
    public class Hunter
    {
        private readonly IResultLogger _resultLogger;
        private readonly IFileProvider _fileProvider;
        private readonly IShortUrlResolver _shortUrlResolver;

        private readonly TransformManyBlock<IFileProvider, SearchItem> _findFilesBlock;
        private readonly TransformBlock<SearchItem, SearchItem> _loadFileTextBlock;
        private readonly TransformManyBlock<SearchItem, SearchItem> _findImagesBlock;
        private readonly ActionBlock<SearchItem> _outputImagesBlock;
        private readonly ActionBlock<SearchItem> _outputProgressBlock;
        private readonly BroadcastBlock<SearchItem> _broadcastFileProcessedBlock;
        private readonly TransformBlock<SearchItem, SearchItem> _followShortUrlsBlock;

        private int _imagesFound;
        private int _errorCount;

        public string SearchFileExtensions { get; set; }
        public int MaxDegreeOfParallelism { get; private set; }
        public int UpdateProgressAfterNumberOfImages { get; set; }

        public Hunter(
            int maxDegreeOfParallelism, 
            IResultLogger resultLogger, 
            IFileProvider fileProvider,
            IShortUrlResolver shortUrlResolver)
        {
            UpdateProgressAfterNumberOfImages = 10;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;

            _resultLogger = resultLogger;
            _fileProvider = fileProvider;
            _shortUrlResolver = shortUrlResolver;

            _broadcastFileProcessedBlock = new BroadcastBlock<SearchItem>(null);
            _findFilesBlock = new TransformManyBlock<IFileProvider, SearchItem>(f => f.GetFilePaths());
            _loadFileTextBlock = new TransformBlock<SearchItem, SearchItem>(s => _fileProvider.GetFile(s), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism });
            _findImagesBlock = new TransformManyBlock<SearchItem, SearchItem>(s => HunterTasks.FindImagesInFile(s), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism });
            _followShortUrlsBlock = new TransformBlock<SearchItem, SearchItem>(i => _shortUrlResolver.ResolveImageShortUrl(i));

            _outputImagesBlock = new ActionBlock<SearchItem>(i => _resultLogger.LogImage(i));
            _outputProgressBlock = new ActionBlock<SearchItem>(i => OutputProgress(i));

            BuildTplNetwork();
        }

        public void Run(string searchPath)
        {
            try
            {
                _resultLogger.OpenLogFile();

                _findFilesBlock.Post(_fileProvider);
                _findFilesBlock.Complete();
                _outputImagesBlock.Completion.Wait();
            }
            catch (AggregateException ae)
            {
                Console.WriteLine();
                Console.WriteLine("One or more errors occured:");
                Console.WriteLine(ae.BuildLog());

            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("An errors occured:");
                Console.WriteLine(ex.BuildLog());
            }
            finally
            {
                _resultLogger.CloseLogFiles();
            }

            Console.WriteLine("Total images found: {0}", _imagesFound);
            Console.WriteLine("Total errors: {0}", _errorCount);
        }
        
        private void BuildTplNetwork()
        {
            ConnectBlocks(_findFilesBlock, _loadFileTextBlock);
            
            ConnectBlocks(_loadFileTextBlock, _findImagesBlock);

            ConnectBlocks(_findImagesBlock, _followShortUrlsBlock);

            ConnectBlocks(_followShortUrlsBlock, _broadcastFileProcessedBlock);
            
            ConnectBlocks(_broadcastFileProcessedBlock, new List<ITargetBlock<SearchItem>>
            {
                _outputImagesBlock,
                _outputProgressBlock
            });
        }

        private void OutputProgress(SearchItem s)
        {
            if (s.Status == SearchItem.Statuses.Failed)
                _errorCount++;
            else
            {
                _imagesFound++;
                if (_imagesFound%UpdateProgressAfterNumberOfImages == 0)
                    Console.WriteLine("Found images: {0}", _imagesFound);
            }
        }

        private void ConnectBlocks<T>(ISourceBlock<T> sourceBlock, ITargetBlock<T> targetBlock)
        {
            sourceBlock.LinkTo(targetBlock);

            sourceBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) targetBlock.Fault(t.Exception);
                else targetBlock.Complete();
            });
        }

        private void ConnectBlocks<T>(ISourceBlock<T> sourceBlock, IList<ITargetBlock<T>> targetBlock)
        {
            foreach (var block in targetBlock)
            {
                sourceBlock.LinkTo(block);
            }

            sourceBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    foreach (var block in targetBlock)
                        block.Fault(t.Exception);
                else
                    foreach (var block in targetBlock)
                        block.Complete();
            });
        }
    }
}