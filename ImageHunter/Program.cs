using System;
using System.Configuration;
using System.Diagnostics;
using ImageHunter.Logging;

namespace ImageHunter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var processorCount = Environment.ProcessorCount;
            var maxDegreeOfParallelism = processorCount * GetThreadsPerProcessor();

            Console.WriteLine("processorCount: {0}", processorCount);
            Console.WriteLine("maxDegreeOfParallelism:{0}", maxDegreeOfParallelism);
            Console.WriteLine();

            var hunter = new Hunter(maxDegreeOfParallelism, new CsvResultLogger())
            {
                SearchFileExtensions = ConfigurationManager.AppSettings["SearchFileExtensions"],
                UpdateProgressAfterNumberOfFiles = 10
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

        private static int GetThreadsPerProcessor()
        {
            const int DefaultThreadsPerProcessor = 4;
            int configValue;
            if (int.TryParse(ConfigurationManager.AppSettings["ThreadsPerProcessor"], out configValue))
                return configValue;
            else
                return DefaultThreadsPerProcessor;
        }
    }
}
    
