using System;

namespace Parser.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string edfFilePath = @"D:\code\psd_csharp\nunit.test\asserts\X.edf";

            Parser p = new Parser();
            (HeaderRecord headerRecord, int handle) = p.Open(edfFilePath);
            double[] buf = p.ReadPhsyicalSamples(handle, 29, 10);
            for (int i = 0; i < buf.Length; i++)
            {
                Console.WriteLine(buf[i]);
            }

            p.Close();
        }
    }
}
