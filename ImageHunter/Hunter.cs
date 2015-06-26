using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using ImageHunter.Logging;

namespace ImageHunter
{
    public class Hunter
    {
        private readonly IResultLogger _resultLogger;

        private readonly TransformManyBlock<string, string> _findFilesBlock;
        private readonly TransformManyBlock<string, FoundImage> _findImagesBlock;
        private readonly ActionBlock<FoundImage> _outputImagesBlock;
        private readonly ActionBlock<FoundImage> _outputProgressBlock;
        private readonly BroadcastBlock<FoundImage> _broadcastFileProcessedBlock; 

        private int _filesProcessed;

        public string SearchFileExtensions { get; set; }
        public int MaxDegreeOfParallelism { get; private set; }
        public int UpdateProgressAfterNumberOfFiles { get; set; }

        public Hunter(IResultLogger resultLogger)
            : this(1, resultLogger)
        {
        }

        public Hunter(int maxDegreeOfParallelism, IResultLogger resultLogger)
        {
            UpdateProgressAfterNumberOfFiles = 10;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            _resultLogger = resultLogger;

            _broadcastFileProcessedBlock = new BroadcastBlock<FoundImage>(null);
            _findFilesBlock = new TransformManyBlock<string, string>(s => HunterTasks.FindFiles(s, SearchFileExtensions));
            _findImagesBlock = new TransformManyBlock<string, FoundImage>((Func<string, IEnumerable<FoundImage>>)HunterTasks.FindImagesInFile, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism });
            _outputImagesBlock = new ActionBlock<FoundImage>((Action<FoundImage>)_resultLogger.LogImage);
            _outputProgressBlock = new ActionBlock<FoundImage>((Action<FoundImage>)OutputProgress);

            BuildTplNetwork();
        }

        public void Run(string searchPath)
        {
            _resultLogger.OpenLogFile();

            _findFilesBlock.Post(searchPath);
            _findFilesBlock.Complete();
            _outputImagesBlock.Completion.Wait();

            _resultLogger.CloseLogFile();

            Console.WriteLine("Total files processed: {0}", _filesProcessed);
        }

        private void BuildTplNetwork()
        {
            _findFilesBlock.LinkTo(_findImagesBlock);

            _findImagesBlock.LinkTo(_broadcastFileProcessedBlock);

            _broadcastFileProcessedBlock.LinkTo(_outputImagesBlock);
            _broadcastFileProcessedBlock.LinkTo(_outputProgressBlock);

            _findFilesBlock.Completion.ContinueWith(t =>
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
            if (_filesProcessed % UpdateProgressAfterNumberOfFiles == 0)
                Console.WriteLine("Found images: {0}", _filesProcessed);
        }

    }
}