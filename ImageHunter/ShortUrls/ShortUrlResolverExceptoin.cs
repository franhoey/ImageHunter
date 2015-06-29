using System;

namespace ImageHunter.ShortUrls
{
    public class ShortUrlResolverExceptoin : HunterException
    {
        public ShortUrlResolverExceptoin()
        {
        }

        public ShortUrlResolverExceptoin(string message)
            : base(message)
        {
        }

        public ShortUrlResolverExceptoin(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}