using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<SearchItem> GetFilePaths()
        {
            return _urls.Select(s => new SearchItem()
            {
                FilePath = s
            });
        }

        public SearchItem GetFile(SearchItem item)
        {
            try
            {
                if (item.Status != SearchItem.Statuses.Ok)
                    return item;

                using (var client = new WebClient())
                {
                    var fileContents = client.DownloadString(item.FilePath);

                    item.FileContents = (ContentsIsTextHtml(client)) ? fileContents : string.Empty;

                    return item;
                }
            }
            catch (Exception ex)
            {
                item.Status = SearchItem.Statuses.Failed;
                item.Error = new FileProviderException("Error getting url", ex);
                return item;
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