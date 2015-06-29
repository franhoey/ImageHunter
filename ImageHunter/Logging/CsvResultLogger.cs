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
            try
            {
                if (_logIsOpen)
                    return;

                if (File.Exists(LOG_FILE_NAME))
                    File.Delete(LOG_FILE_NAME);

                _logWriter = File.CreateText(LOG_FILE_NAME);
                _logWriter.WriteLine("File,Image");

                _logIsOpen = true;
            }
            catch (Exception ex)
            {
                throw new LoggingException("Error opening logfile", ex);
            }
        }

        public void CloseLogFile()
        {
            try
            {
                if (_logIsOpen)
                {
                    _logWriter.Flush();
                    _logWriter.Close();
                    _logIsOpen = false;
                }
            }
            catch (Exception ex)
            {
                throw new LoggingException("Error closing logfile", ex);
            }
        }

        public void LogImage(FoundImage image)
        {
            try
            {
                _logWriter.WriteLine("{0},{1}", image.FileName, image.ImageName);
            }
            catch (Exception ex)
            {
                throw new LoggingException(string.Format("Error logging result: {0},{1}", image.FileName, image.ImageName), ex);
            }
            
        }

        public void Dispose()
        {
            CloseLogFile();
        }
    }
}