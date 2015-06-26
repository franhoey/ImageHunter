using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using ImageHunter.FileProvider;
using ImageHunter.Logging;

namespace ImageHunter
{
    public class Hunter
    {
        private readonly IResultLogger _resultLogger;
        private readonly IFileProvider _fileProvider;

        private readonly TransformManyBlock<IFileProvider, string> _findFilesBlock;
        private readonly TransformBlock<string, SearchableFile> _loadFileTextBlock;
        private readonly TransformManyBlock<SearchableFile, FoundImage> _findImagesBlock;
        private readonly ActionBlock<FoundImage> _outputImagesBlock;
        private readonly ActionBlock<FoundImage> _outputProgressBlock;
        private readonly BroadcastBlock<FoundImage> _broadcastFileProcessedBlock; 

        private int _filesProcessed;

        public string SearchFileExtensions { get; set; }
        public int MaxDegreeOfParallelism { get; private set; }
        public int UpdateProgressAfterNumberOfImages { get; set; }

        public Hunter(int maxDegreeOfParallelism, IResultLogger resultLogger, IFileProvider fileProvider)
        {
            UpdateProgressAfterNumberOfImages = 10;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;

            _resultLogger = resultLogger;
            _fileProvider = fileProvider;

            _broadcastFileProcessedBlock = new BroadcastBlock<FoundImage>(null);
            _findFilesBlock = new TransformManyBlock<IFileProvider, string>(f => f.GetFilePaths());
            _loadFileTextBlock = new TransformBlock<string, SearchableFile>(s => _fileProvider.GetFile(s), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism });
            _findImagesBlock = new TransformManyBlock<SearchableFile, FoundImage>(s => HunterTasks.FindImagesInFile(s), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism });
            _outputImagesBlock = new ActionBlock<FoundImage>(i => _resultLogger.LogImage(i));
            _outputProgressBlock = new ActionBlock<FoundImage>(i => OutputProgress(i));

            BuildTplNetwork();
        }

        public void Run(string searchPath)
        {
            _resultLogger.OpenLogFile();

            _findFilesBlock.Post(_fileProvider);
            _findFilesBlock.Complete();
            _outputImagesBlock.Completion.Wait();

            _resultLogger.CloseLogFile();

            Console.WriteLine("Total images found: {0}", _filesProcessed);
        }

        private void BuildTplNetwork()
        {
            _findFilesBlock.LinkTo(_loadFileTextBlock);

            _loadFileTextBlock.LinkTo(_findImagesBlock);

            _findImagesBlock.LinkTo(_broadcastFileProcessedBlock);

            _broadcastFileProcessedBlock.LinkTo(_outputImagesBlock);
            _broadcastFileProcessedBlock.LinkTo(_outputProgressBlock);

            _findFilesBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)_loadFileTextBlock).Fault(t.Exception);
                else _loadFileTextBlock.Complete();
            });
            _loadFileTextBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)_findImagesBlock).Fault(t.Exception);
                else _findImagesBlock.Complete();
            });
            _findImagesBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)_broadcastFileProcessedBlock).Fault(t.Exception);
                else _broadcastFileProcessedBlock.Complete();
            });
            _broadcastFileProcessedBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)_outputImagesBlock).Fault(t.Exception);
                else _outputImagesBlock.Complete();
            });
        }

        private void OutputProgress(FoundImage s)
        {
            _filesProcessed++;
            if (_filesProcessed % UpdateProgressAfterNumberOfImages == 0)
                Console.WriteLine("Found images: {0}", _filesProcessed);
        }

    }
}