using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace ImageHunter
{
    public static class HunterTasks
    {
        private static readonly Regex FindImageRegex = new Regex("<img.*?src=[\"'](?<src>(http|//)[^\"']+)[\"']", RegexOptions.Compiled);

        public static IEnumerable<FoundImage> FindImagesInFile(SearchableFile file)
        {
            var matches = FindImageRegex.Matches(file.FileContents);
            foreach (Match match in matches)
            {
                yield return new FoundImage() { FileName = file.FilePath, ImageName = match.Groups["src"].Value };
            }
        }
    }
}