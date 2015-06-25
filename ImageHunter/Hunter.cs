using System;
using System.IO;

namespace ImageHunter
{
    public class Hunter
    {
        public string SearchFileExtensions { get; set; }
        public string SearchPath { get; set; }

        public void Run()
        {
            HuntFolder(SearchPath);
        }

        private void HuntFolder(string path)
        {
            if (!Directory.Exists(path))
                return;

            var files = Directory.GetFiles(path, SearchFileExtensions);
            foreach (var file in files)
                HuntFile(file);

            var subDirectories = Directory.GetDirectories(path);
            foreach (var subDirectory in subDirectories)
            {
                HuntFolder(subDirectory);
            }
        }

        private void HuntFile(string path)
        {
            if (!File.Exists(path))
                return;

            Console.WriteLine(path);
        }
    }
}