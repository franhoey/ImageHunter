using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHunter
{
    class Program
    {
        static void Main(string[] args)
        {
            var hunter = new Hunter()
            {
                SearchPath = @"C:\Projects\Yara\yara-com\src\Web\BB.Yara.Com\BB.Yara.Web.Com",
                SearchFileExtensions = "*.aspx"
            };
            hunter.Run();

            Console.WriteLine();
            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
