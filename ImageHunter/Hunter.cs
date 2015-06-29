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

        private readonly TransformManyBlock<IFileProvider, string> _findFilesBlock;
        private readonly TransformBlock<string, SearchableFile> _loadFileTextBlock;
        private readonly TransformManyBlock<SearchableFile, FoundImage> _findImagesBlock;
        private readonly ActionBlock<FoundImage> _outputImagesBlock;
        private readonly ActionBlock<FoundImage> _outputProgressBlock;
        private readonly BroadcastBlock<FoundImage> _broadcastFileProcessedBlock;
        private readonly TransformBlock<FoundImage, FoundImage> _followShortUrlsBlock;

        private int _filesProcessed;

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

            _broadcastFileProcessedBlock = new BroadcastBlock<FoundImage>(null);
            _findFilesBlock = new TransformManyBlock<IFileProvider, string>(f => f.GetFilePaths());
            _loadFileTextBlock = new TransformBlock<string, SearchableFile>(s => _fileProvider.GetFile(s), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism });
            _findImagesBlock = new TransformManyBlock<SearchableFile, FoundImage>(s => HunterTasks.FindImagesInFile(s), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism });
            _outputImagesBlock = new ActionBlock<FoundImage>(i => _resultLogger.LogImage(i));
            _followShortUrlsBlock = new TransformBlock<FoundImage, FoundImage>(i =>
            {
                if (!_shortUrlResolver.IsShortUrl(i.ImageName))
                    return i;

                i.ImageName = _shortUrlResolver.ResolveUrl(i.ImageName);
                return i;
            });

            _outputProgressBlock = new ActionBlock<FoundImage>(i => OutputProgress(i));

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
                OutputAggregationErrors(ae);

            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("An errors occured:");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _resultLogger.CloseLogFile();
            }
            
            Console.WriteLine("Total images found: {0}", _filesProcessed);
        }

        private void OutputAggregationErrors(AggregateException ae)
        {
            foreach (var e in ae.InnerExceptions)
            {
                var subAe = e as AggregateException;
                if(subAe != null)
                    OutputAggregationErrors(subAe);
                else
                    Console.WriteLine(e.Message);
            }
        }

        private void BuildTplNetwork()
        {
            ConnectBlocks(_findFilesBlock, _loadFileTextBlock);
            
            ConnectBlocks(_loadFileTextBlock, _findImagesBlock);

            ConnectBlocks(_findImagesBlock, _followShortUrlsBlock);

            ConnectBlocks(_followShortUrlsBlock, _broadcastFileProcessedBlock);
            
            ConnectBlocks(_broadcastFileProcessedBlock, new List<ITargetBlock<FoundImage>>
            {
                _outputImagesBlock,
                _outputProgressBlock
            });
        }

        private void OutputProgress(FoundImage s)
        {
            _filesProcessed++;
            if (_filesProcessed % UpdateProgressAfterNumberOfImages == 0)
                Console.WriteLine("Found images: {0}", _filesProcessed);
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