using System;
using System.IO;

namespace ImageHunter.Logging
{
    public class CsvResultLogger : IResultLogger, IDisposable
    {
        private const string LOG_FILE_NAME = "output.csv";

        private StreamWriter _logWriter;
        private bool _logIsOpen;

        public void OpenLogFile()
        {
            if (_logIsOpen)
                return;

            if (File.Exists(LOG_FILE_NAME))
                File.Delete(LOG_FILE_NAME);

            _logWriter = File.CreateText(LOG_FILE_NAME);
            _logWriter.WriteLine("File,Image");

            _logIsOpen = true;
        }

        public void CloseLogFile()
        {
            if (_logIsOpen)
            {
                _logWriter.Flush();
                _logWriter.Close();
                _logIsOpen = false;
            }
        }

        public void LogImage(FoundImage image)
        {
            _logWriter.WriteLine("{0},{1}", image.FileName, image.ImageName);
        }

        public void Dispose()
        {
            CloseLogFile();
        }
    }
}