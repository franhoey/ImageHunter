using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ImageHunter.FileProvider;
using ImageHunter.Logging;
using ImageHunter.ShortUrls;

namespace ImageHunter
{
    internal class Program
    {
        const int DEFAULT_THREADS_PER_PROCESSOR = 4;
        const int DEFAULT_PROGRESS_UPDATE_EVERY_N_IMAGES = 10;

        static void Main(string[] args)
        {
            var processorCount = Environment.ProcessorCount;
            var maxDegreeOfParallelism = processorCount * GetConfigInt("ThreadsPerProcessor", DEFAULT_THREADS_PER_PROCESSOR);

            Console.WriteLine("processorCount: {0}", processorCount);
            Console.WriteLine("maxDegreeOfParallelism:{0}", maxDegreeOfParallelism);
            Console.WriteLine();

            var stopwatch = new Stopwatch();

            IFileProvider fileProvider;

            //fileProvider = GetLocalFileProvider();
            fileProvider = GetHttpFileProvider();

            IShortUrlResolver shortUrlResolver = GetShortUrlResolver();

            using (var logger = new CsvResultLogger())
            {

                var hunter = new Hunter(maxDegreeOfParallelism, logger, fileProvider, shortUrlResolver)
                {
                    SearchFileExtensions = ConfigurationManager.AppSettings["SearchFileExtensions"],
                    UpdateProgressAfterNumberOfImages =
                        GetConfigInt("ProgressUpdateEveryNImages", DEFAULT_PROGRESS_UPDATE_EVERY_N_IMAGES)
                };

                stopwatch.Start();
                hunter.Run(ConfigurationManager.AppSettings["SearchPath"]);
                stopwatch.Stop();
            }

            Console.WriteLine();
            Console.WriteLine("Time elapsed:{0}", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        private static int GetConfigInt(string key,int defaultValue)
        {
            int configValue;
            if (int.TryParse(ConfigurationManager.AppSettings[key], out configValue))
                return configValue;
            else
                return defaultValue;
        }

        private static IFileProvider GetLocalFileProvider()
        {
            return new LocalFileProvider(ConfigurationManager.AppSettings["SearchPath"], ConfigurationManager.AppSettings["SearchFileExtensions"]);
        }

        private static IFileProvider GetHttpFileProvider()
        {
            return new HttpFileProvider(LoadUrlList());
        }

        private static List<string> LoadUrlList()
        {
            var filePath = ConfigurationManager.AppSettings["SearchUrls"];
            if (!File.Exists(filePath))
                throw new ConfigurationErrorsException("Cannot find file containing urls at path in configuration \"SearchUrls\"");

            return File.ReadAllLines(filePath)
                .Where(l => !string.IsNullOrEmpty(l))
                .ToList();
        }

        private static IShortUrlResolver GetShortUrlResolver()
        {
            return new ShortUrlResolver(new List<string>
            {
                "yaraurl.com"
            });
        }
    }
}
    
