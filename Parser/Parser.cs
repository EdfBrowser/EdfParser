// https://www.edfplus.info/specs/edf.html
using System;
using System.IO;

namespace Parser
{
    public interface IParser
    {
        (HeaderRecord, int) Open(string edfFilePath);
    }

    public class Parser : IParser
    {
        private const int MAXPARSER = 1000;
        private readonly Reader[] _readers;

        public Parser()
        {
            _readers = new Reader[MAXPARSER];
        }

        public (HeaderRecord, int) Open(string edfFilePath)
        {
            Reader reader = new Reader(File.OpenRead(edfFilePath));
            int handle = -1;

            for (int i = 0; i < MAXPARSER; i++)
            {
                if (_readers[i] == null)
                {
                    _readers[i] = reader;
                    handle = i;
                    break;
                }
            }

            return (reader.ReadHeaderRecord(), handle);
        }

        public void Close()
        {
            for (int i = 0; i < MAXPARSER; i++)
            {
                _readers[i]?.Close();
            }
        }

        public double[] ReadPhsyicalSamples(int handle, int signal, long count)
        {
            if (handle < 0 || handle > MAXPARSER)
                throw new ArgumentException($"The {nameof(handle)} value is out of the valid range!");


            return _readers[handle].ReadPhsyicalSamples(signal, count);
        }
    }
}
