using System.Collections.Generic;
using System.Net;

namespace ImageHunter.FileProvider
{
    public class HttpFileProvider : IFileProvider
    {
        private readonly IList<string> _urls;

        public HttpFileProvider(IList<string> urls)
        {
            _urls = urls;
        }

        public IEnumerable<string> GetFilePaths()
        {
            return _urls;
        }

        public SearchableFile GetFile(string path)
        {
            using (var client = new WebClient())
            {
                return new SearchableFile()
                {
                    FilePath = path,
                    FileContents = client.DownloadString(path)
                };
            }
        }
    }
}