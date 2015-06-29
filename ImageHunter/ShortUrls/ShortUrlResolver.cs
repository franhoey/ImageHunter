using System;
using System.Collections.Generic;
using System.Net;

namespace ImageHunter.ShortUrls
{
    public class ShortUrlResolver : IShortUrlResolver
    {
        private readonly IList<string> _shortUrlHostnames;

        public ShortUrlResolver(IList<string> shortUrlHostnames)
        {
            _shortUrlHostnames = shortUrlHostnames;
        }

        public string ResolveUrl(string url)
        {
            if (!IsShortUrl(url))
                return url;

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

                return string.IsNullOrEmpty(responseLocation)
                    ? url
                    : responseLocation;
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