using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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

            var hunter = new Hunter(maxDegreeOfParallelism)
            {
                SearchPath = @"C:\Projects\Yara\yara-com\src\Web\BB.Yara.Com\BB.Yara.Web.Com",
                SearchFileExtensions = "*.aspx"
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            hunter.Run();

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Time elapsed:{0}", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
    
