using System;

namespace Parser.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string edfFilePath = @"D:\code\psd_csharp\nunit.test\asserts\X.edf";

            using (Reader reader = new Reader(edfFilePath))
            {
                reader.ReadHeader();
                double[] buf = reader.ReaderData(0, 501);
                for (int i = 0; i < buf.Length; i++)
                {
                    Console.WriteLine(buf[i]);
                }
            }
        }
    }
}
