using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace ImageHunter
{
    public class Hunter : IDisposable
    {
        private const string LOG_FILE_NAME = "output.csv";

        private readonly TransformManyBlock<string, string> _findFilesBlock;
        private readonly TransformManyBlock<string, FoundImage> _findImagesBlock;
        private readonly ActionBlock<FoundImage> _outputImagesBlock;
        private readonly ActionBlock<string> _outputProgressBlock;
        private readonly BroadcastBlock<string> _broadcastFileBlock; 

        private StreamWriter _logWriter;
        private bool _logIsOpen;
        private int _filesProcessed;
        private readonly Regex _findImageRegex = new Regex("<img.*?src=[\"'](?<src>[^\"']+)[\"']", RegexOptions.Compiled);

        public string SearchFileExtensions { get; set; }
        public string SearchPath { get; set; }
        public int MaxDegreeOfParallelism { get; private set; }
        public int UpdateProgressAfterNumberOfFiles { get; set; }
       
        public Hunter() : this(1)
        {
        }

        public Hunter(int maxDegreeOfParallelism)
        {
            UpdateProgressAfterNumberOfFiles = 10;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            _logIsOpen = false;
            _broadcastFileBlock = new BroadcastBlock<string>(null);
            _findFilesBlock = new TransformManyBlock<string, string>((Func<string, IEnumerable<string>>)FindFiles);
            _findImagesBlock = new TransformManyBlock<string, FoundImage>((Func<string, IEnumerable<FoundImage>>)FindImagesInFile, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism });
            _outputImagesBlock = new ActionBlock<FoundImage>((Action<FoundImage>)OutputImages);
            _outputProgressBlock = new ActionBlock<string>((Action<string>) OutputProgress);
            BuildTplNetwork();
        }

        private void OutputProgress(string s)
        {
            _filesProcessed ++;
            if (_filesProcessed % UpdateProgressAfterNumberOfFiles == 0)
                Console.WriteLine("Processed {0} Files", _filesProcessed);
        }

        private IEnumerable<string> FindFiles(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, SearchFileExtensions);
            foreach (var file in files)
                yield return file;
            
            var subDirectories = Directory.GetDirectories(folderPath);
            foreach (var subDirectory in subDirectories)
            {
                foreach (var file in FindFiles(subDirectory))
                {
                    yield return file;
                }
            }
        }

        private IEnumerable<FoundImage> FindImagesInFile(string filePath)
        {
            var fileText = File.ReadAllText(filePath);
            var matches = _findImageRegex.Matches(fileText);
            foreach (Match match in matches)
            {
                yield return new FoundImage() {FileName = filePath, ImageName = match.Groups["src"].Value};
            }
        }

        private void OutputImages(FoundImage image)
        {
            _logWriter.WriteLine("{0},{1}",image.FileName, image.ImageName);
        }

        public void Run()
        {
            OpenLogFile();

            _findFilesBlock.Post(SearchPath);
            _findFilesBlock.Complete();
            _outputImagesBlock.Completion.Wait();

            CloseLogFile();
        }

        private void BuildTplNetwork()
        {
            _findFilesBlock.LinkTo(_broadcastFileBlock);
            _broadcastFileBlock.LinkTo(_findImagesBlock);
            _broadcastFileBlock.LinkTo(_outputProgressBlock);
            _findImagesBlock.LinkTo(_outputImagesBlock);

            _findFilesBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)_broadcastFileBlock).Fault(t.Exception);
                else _broadcastFileBlock.Complete();
            });
            _broadcastFileBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)_findImagesBlock).Fault(t.Exception);
                else _findImagesBlock.Complete();
            });
            _findImagesBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock) _outputImagesBlock).Fault(t.Exception);
                else _outputImagesBlock.Complete();
            });
        }

        private void OpenLogFile()
        {
            if (_logIsOpen)
                return;
            
            if(File.Exists(LOG_FILE_NAME))
                File.Delete(LOG_FILE_NAME);

            _logWriter = File.CreateText(LOG_FILE_NAME);
            _logWriter.WriteLine("File,Image");

            _logIsOpen = true;
        }

        private void CloseLogFile()
        {
            if (_logIsOpen)
            {
                _logWriter.Flush();
                _logWriter.Close();
                _logIsOpen = false;
            }
        }

        public void Dispose()
        {
            CloseLogFile();
        }
    }
}