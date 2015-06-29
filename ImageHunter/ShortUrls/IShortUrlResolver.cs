namespace ImageHunter.ShortUrls
{
    public interface IShortUrlResolver
    {
        string ResolveUrl(string url);
        bool IsShortUrl(string url);
    }
}