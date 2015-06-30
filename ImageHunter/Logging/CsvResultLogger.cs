using System;
using System.IO;

namespace ImageHunter.Logging
{
    public class CsvResultLogger : IResultLogger, IDisposable
    {
        private const string LOG_FILE_NAME = "output.csv";
        private const string ERRORS_FILE_NAME = "errors.csv";

        private StreamWriter _logWriter;
        private StreamWriter _errorWriter;
        private bool _filesAreOpen;

        public void OpenLogFile()
        {
            try
            {
                if (_filesAreOpen)
                    return;

                if (File.Exists(LOG_FILE_NAME))
                    File.Delete(LOG_FILE_NAME);

                if (File.Exists(ERRORS_FILE_NAME))
                    File.Delete(ERRORS_FILE_NAME);

                _logWriter = File.CreateText(LOG_FILE_NAME);
                _logWriter.WriteLine("File,Image,ImageShortUrl");

                _errorWriter = File.CreateText(ERRORS_FILE_NAME);

                _filesAreOpen = true;
            }
            catch (Exception ex)
            {
                throw new LoggingException("Error opening logfile", ex);
            }
        }

        public void CloseLogFiles()
        {
            try
            {
                if (_filesAreOpen)
                {
                    _logWriter.Flush();
                    _logWriter.Close();

                    _errorWriter.Flush();
                    _errorWriter.Close();

                    _filesAreOpen = false;
                }
            }
            catch (Exception ex)
            {
                throw new LoggingException("Error closing logfile", ex);
            }
        }

        public void LogImage(SearchItem item)
        {
            try
            {
                if (item.Status == SearchItem.Statuses.Failed)
                    LogError(item);
                else
                    LogSuccess(item);
            }
            catch (Exception ex)
            {
                throw new LoggingException(string.Format("Error logging result: {0},{1}", item.FilePath, item.ImageUrl), ex);
            }   
        }

        private void LogSuccess(SearchItem item)
        {
            _logWriter.WriteLine("\"{0}\",\"{1}\",\"{2}\"", item.FilePath, item.ImageUrl, item.ShortUrl);
        }

        private void LogError(SearchItem item)
        {
            _errorWriter.WriteLine("File: {0}", item.FilePath);
            _errorWriter.WriteLine("Image: {0}", item.ImageUrl);
            _errorWriter.WriteLine("ShortUrl: {0}", item.ShortUrl);
            _errorWriter.WriteLine("Error: {0}", item.Error.BuildLog());
            _errorWriter.WriteLine();
        }

        public void Dispose()
        {
            CloseLogFiles();
        }
    }
}