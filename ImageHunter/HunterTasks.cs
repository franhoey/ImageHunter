using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace ImageHunter
{
    public static class HunterTasks
    {
        private static readonly Regex FindImageRegex = new Regex("<img.*?src=[\"'](?<src>(http|//)[^\"']+)[\"']", RegexOptions.Compiled);

        public static IEnumerable<SearchItem> FindImagesInFile(SearchItem item)
        {
            if (item.Status != SearchItem.Statuses.Ok)
                yield return item;
            else
            {
                var matches = FindImageRegex.Matches(item.FileContents);
                foreach (Match match in matches)
                {
                    var newItem = item.Clone();
                    newItem.ImageUrl = match.Groups["src"].Value;
                    yield return newItem;
                }
            }
        }
    }
}