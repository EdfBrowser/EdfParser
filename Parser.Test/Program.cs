using System;
using System.Diagnostics;

namespace Parser.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string edfFilePath = @"D:\code\psd_csharp\nunit.test\asserts\X.edf";

            using (Reader reader = new Reader(edfFilePath))
            {
                double[] buf = new double[1000];
                sw.Restart();

                int count = reader.ReadDataAsync(27, buf).GetAwaiter().GetResult();

                //double[] buf = reader.ReadFromMemoryMappingFile(edfFilePath, 0, 503);

                sw.Stop();

                Console.WriteLine(sw.ElapsedMilliseconds);
                Console.WriteLine(count);
                for (int i = 0; i < count; i++)
                {
                    Console.WriteLine(buf[i]);
                }
            }
        }
    }
}
