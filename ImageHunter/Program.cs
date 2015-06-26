using System;
using System.Diagnostics;
using ImageHunter.Logging;

namespace ImageHunter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var processorCount = Environment.ProcessorCount;
            var maxDegreeOfParallelism = processorCount * 4;
            Console.WriteLine("processorCount: {0}", processorCount);
            Console.WriteLine("maxDegreeOfParallelism:{0}", maxDegreeOfParallelism);
            Console.WriteLine();

            var hunter = new Hunter(maxDegreeOfParallelism, new CsvResultLogger())
            {
                SearchFileExtensions = "*.aspx",
                UpdateProgressAfterNumberOfFiles = 10
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            hunter.Run(@"C:\Projects\Yara\yara-com\src\Web\BB.Yara.Com\BB.Yara.Web.Com");

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Time elapsed:{0}", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
    
