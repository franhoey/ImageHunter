using System;
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
                var fileContents = client.DownloadString(path);

                return new SearchableFile()
                {
                    FilePath = path,
                    FileContents = (ContentsIsTextHtml(client))?fileContents:string.Empty
                };
            }
        }

        private bool ContentsIsTextHtml(WebClient client)
        {
            if (client.ResponseHeaders == null)
                return false;

            var contentType = client.ResponseHeaders[HttpResponseHeader.ContentType];
            return contentType.ToLower().Contains("text/html");
        }
    }
}