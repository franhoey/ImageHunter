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

        public string ResolveUrl(string url)
        {
            try
            {
                if (!IsShortUrl(url))
                    return url;

                if (CachedUrls.ContainsKey(url))
                    return CachedUrls[url];

                var request = WebRequest.CreateHttp(url);
                request.MaximumAutomaticRedirections = 1;
                request.AllowAutoRedirect = false;
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response == null)
                        return url;
                    if (!IsRedirectResponse(response.StatusCode))
                        return url;

                    var responseLocation = response.GetResponseHeader("Location");

                    var retval = string.IsNullOrEmpty(responseLocation)
                        ? url
                        : responseLocation;

                    CachedUrls.TryAdd(url, retval);

                    return retval;
                }
            }
            catch (Exception ex)
            {
                throw new ShortUrlResolverExceptoin(String.Format("Error resolving short url: {0}", url), ex);
            }
            
        }

        private bool IsRedirectResponse(HttpStatusCode statusCode)
        {
            var statusCodeInt = (int) statusCode;
            return statusCodeInt >= 300 && statusCodeInt <= 399;
        }

        public bool IsShortUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return false;

            return _shortUrlHostnames.Contains(uri.Host);
        }

    }
}