using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace ImageHunter.ShortUrls
{
    public class ShortUrlResolver : IShortUrlResolver
    {
        private readonly IList<string> _shortUrlHostnames;
        private static readonly ConcurrentDictionary<string, string> CachedUrls = new ConcurrentDictionary<string, string>(); 

        public ShortUrlResolver(IList<string> shortUrlHostnames)
        {
            _shortUrlHostnames = shortUrlHostnames;
        }

        public SearchItem ResolveImageShortUrl(SearchItem item)
        {
            try
            {
                if (item.Status != SearchItem.Statuses.Ok)
                    return item;

                if (!IsShortUrl(item.ImageUrl))
                    return item;

                item.ShortUrl = item.ImageUrl;

                if (CachedUrls.ContainsKey(item.ImageUrl))
                {
                    item.ImageUrl = CachedUrls[item.ImageUrl];
                    return item;
                }

                var request = WebRequest.CreateHttp(item.ImageUrl);
                request.MaximumAutomaticRedirections = 1;
                request.AllowAutoRedirect = false;
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response == null)
                        return item;
                    if (!IsRedirectResponse(response.StatusCode))
                        return item;

                    var responseLocation = response.GetResponseHeader("Location");

                    if (!string.IsNullOrEmpty(responseLocation))
                    {
                        CachedUrls.TryAdd(item.ImageUrl, responseLocation);
                        item.ImageUrl = responseLocation;
                    }
                    else
                        CachedUrls.TryAdd(item.ImageUrl, item.ImageUrl);
                    
                    return item;
                }
            }
            catch (Exception ex)
            {
                item.Status = SearchItem.Statuses.Failed;
                item.Error = new ShortUrlResolverExceptoin(String.Format("Error resolving short item: {0}", item), ex);
                return item;
            }
        }

        private bool IsRedirectResponse(HttpStatusCode statusCode)
        {
            var statusCodeInt = (int) statusCode;
            return statusCodeInt >= 300 && statusCodeInt <= 399;
        }

        private bool IsShortUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return false;

            return _shortUrlHostnames.Contains(uri.Host);
        }

    }
}