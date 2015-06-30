namespace ImageHunter.ShortUrls
{
    public interface IShortUrlResolver
    {
        SearchItem ResolveImageShortUrl(SearchItem item);
    }
}