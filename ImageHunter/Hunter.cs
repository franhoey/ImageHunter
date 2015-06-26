using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace ImageHunter
{
    public class Hunter : IDisposable
    {
        private const string LOG_FILE_NAME = "output.log";

        private readonly TransformManyBlock<string, string> _findFilesBlock;
        private readonly TransformManyBlock<string, string> _findImagesBlock;
        private readonly ActionBlock<string> _outputImagesBlock;

        private StreamWriter _logWriter;
        private bool _logIsOpen;

        public string SearchFileExtensions { get; set; }
        public string SearchPath { get; set; }
        public int MaxDegreeOfParallelism { get; private set; }
       
        public Hunter() : this(1)
        {
        }

        public Hunter(int maxDegreeOfParallelism)
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            _logIsOpen = false;
            _findFilesBlock = new TransformManyBlock<string, string>((Func<string, IEnumerable<string>>)FindFiles);
            _findImagesBlock = new TransformManyBlock<string, string>((Func<string, IEnumerable<string>>)FindImagesInFile, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism });
            _outputImagesBlock = new ActionBlock<string>((Action<string>) OutputImages);
            BuildTplNetwork();
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

        private IEnumerable<string> FindImagesInFile(string filePath)
        {
            Thread.Sleep(1000);
            return filePath.Split('\\');
        }

        private void OutputImages(string imagePath)
        {
            if(imagePath.EndsWith(".aspx"))
                _logWriter.WriteLine(imagePath);
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
            _findFilesBlock.LinkTo(_findImagesBlock);
            _findImagesBlock.LinkTo(_outputImagesBlock);

            _findFilesBlock.Completion.ContinueWith(t =>
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