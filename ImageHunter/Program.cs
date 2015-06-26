using System;
using System.Configuration;
using System.Diagnostics;
using ImageHunter.Logging;

namespace ImageHunter
{
    internal class Program
    {
        const int DEFAULT_THREADS_PER_PROCESSOR = 4;
        const int DEFAULT_PROGRESS_UPDATE_EVERY_N_FILES = 10;

        static void Main(string[] args)
        {
            var processorCount = Environment.ProcessorCount;
            var maxDegreeOfParallelism = processorCount * GetConfigInt("ThreadsPerProcessor", DEFAULT_THREADS_PER_PROCESSOR);

            Console.WriteLine("processorCount: {0}", processorCount);
            Console.WriteLine("maxDegreeOfParallelism:{0}", maxDegreeOfParallelism);
            Console.WriteLine();

            var hunter = new Hunter(maxDegreeOfParallelism, new CsvResultLogger())
            {
                SearchFileExtensions = ConfigurationManager.AppSettings["SearchFileExtensions"],
                UpdateProgressAfterNumberOfFiles = GetConfigInt("ProgressUpdateEveryNFiles", DEFAULT_PROGRESS_UPDATE_EVERY_N_FILES)
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            hunter.Run(ConfigurationManager.AppSettings["SearchPath"]);

            stopwatch.Stop();

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
    }
}
    
