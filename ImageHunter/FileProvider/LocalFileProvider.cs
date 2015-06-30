using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public IEnumerable<SearchItem> GetFilePaths()
        {
            try
            {
                return FindFiles(_filePath)
                    .Select(s => new SearchItem() { FilePath = s});
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

        public SearchItem GetFile(SearchItem item)
        {
            try
            {
                if (item.Status != SearchItem.Statuses.Ok)
                    return item;

                item.FileContents = File.ReadAllText(item.FilePath);
                return item;
            }
            catch (Exception ex)
            {
                item.Status = SearchItem.Statuses.Failed;
                item.Error = new FileProviderException("Error getting file", ex);
                return item;
            }
        }
    }
}