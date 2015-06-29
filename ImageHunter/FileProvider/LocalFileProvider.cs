using System;
using System.Collections.Generic;
using System.IO;

namespace ImageHunter.FileProvider
{
    public class LocalFileProvider : IFileProvider
    {
        private readonly string _filePath;
        private readonly string _extensionFilter;

        public LocalFileProvider(string filePath, string extensionFilter)
        {
            _filePath = filePath;
            _extensionFilter = extensionFilter;
        }

        public IEnumerable<string> GetFilePaths()
        {
            try
            {
                return FindFiles(_filePath);
            }
            catch (Exception ex)
            {
                throw new FileProviderException("Error file paths", ex);
            }
        }

        private IEnumerable<string> FindFiles(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, _extensionFilter);
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

        public SearchableFile GetFile(string path)
        {
            try
            {
                return new SearchableFile()
                {
                    FilePath = path,
                    FileContents = File.ReadAllText(path)
                };
            }
            catch (Exception ex)
            {
                throw new FileProviderException(string.Format("Error getting file: {0}", path), ex);
            }
        }
    }
}