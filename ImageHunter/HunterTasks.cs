using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ImageHunter
{
    public static class HunterTasks
    {
        private static readonly Regex FindImageRegex = new Regex("<img.*?src=[\"'](?<src>[^\"']+)[\"']", RegexOptions.Compiled);

        public static IEnumerable<string> FindFiles(string folderPath, string fileExtensions)
        {
            var files = Directory.GetFiles(folderPath, fileExtensions);
            foreach (var file in files)
                yield return file;

            var subDirectories = Directory.GetDirectories(folderPath);
            foreach (var subDirectory in subDirectories)
            {
                foreach (var file in FindFiles(subDirectory, fileExtensions))
                {
                    yield return file;
                }
            }
        }

        public static IEnumerable<FoundImage> FindImagesInFile(string filePath)
        {
            var fileText = File.ReadAllText(filePath);
            var matches = FindImageRegex.Matches(fileText);
            foreach (Match match in matches)
            {
                yield return new FoundImage() { FileName = filePath, ImageName = match.Groups["src"].Value };
            }
        }
    }
}