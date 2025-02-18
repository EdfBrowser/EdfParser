using System;
using System.IO;

namespace Parser
{
    internal class HeaderReader
    {
        internal HeaderRecord Read(BinaryReader reader)
        {
            var header = new HeaderRecord();
            header.Read(reader);
            return header;
        }
    }

    internal class DataReader
    {
        internal double[] Read(BinaryReader reader, HeaderRecord headerRecord,
            int signal, int count)
        {
            int ns = headerRecord.NumberOfSignals.Value;

            if (signal < 0 || signal >= ns)
                throw new ArgumentException($"The {nameof(signal)} value is out of the valid range!");

            if (count < 0)
                throw new ArgumentException($"The {nameof(count)} value is invalid!");

            if (count == 0)
                return Array.Empty<double>();

            int bytesPerSmp = 2;

            int channel = headerRecord.MappedSignals[signal];
            int smpPos = headerRecord.SamplePos[channel];
            int smpPerRecord = headerRecord.NumberOfSamplesInDataRecord.Value[channel];
            int smpInFile = smpPerRecord * headerRecord.NumberOfDataRecords.Value;

            // 如果 count 大于文件内剩余的样本数，则调整 count 为文件剩余的样本数
            if ((smpPos + count) > smpInFile)
            {
                count = smpInFile - smpPos;
                if (count == 0)
                    return Array.Empty<double>();

                if (count < 0)
                    throw new ArgumentException("Invalid calculation: the count value became negative.");
            }

            double[] buf = new double[count];

            // 头部分大小
            long offset = 256 + ns * 256;
            // 之前读取到第几块了
            offset += (smpPos / smpPerRecord) * headerRecord.RecordSize;
            // 该channel在块中偏移多少字节
            offset += headerRecord.BufOffset[channel];
            // 从当前块开始偏移了多少字节
            offset += (smpPos % smpPerRecord) * bytesPerSmp;

            reader.BaseStream.Seek(offset, SeekOrigin.Begin);

            // 假如当前的位置是dataRecord的末尾，需要跳到下一块的开始
            long jump = headerRecord.RecordSize - (smpPerRecord * bytesPerSmp);

            for (int i = 0; i < count; i++)
            {
                // 在当前块中读完了该channel的所有samples
                // 并且需要排除当前指向channel数据块开头的情况
                if ((smpPos % smpPerRecord == 0) && i > 0)
                    reader.BaseStream.Seek(jump, SeekOrigin.Current);

                byte[] smp = reader.ReadBytes(bytesPerSmp);
                double val = BitConverter.ToInt16(smp, 0);
                buf[i] = headerRecord.BitValues[channel] * (val + headerRecord.Offsets[channel]);

                smpPos++;
            }

            headerRecord.SamplePos[channel] = smpPos;

            return buf;
        }
    }

    public class Reader : IDisposable
    {
        private readonly Stream _stream;
        private readonly BinaryReader _binaryReader;
        private readonly HeaderReader _headerReader;
        private readonly DataReader _dataReader;

        private HeaderRecord _headerRecord;
        private bool _disposed;

        public Reader(Stream stream)
        {
            _disposed = false;

            _stream = stream
                ?? throw new ArgumentNullException($"{nameof(stream)} is null!");
            _binaryReader = new BinaryReader(stream);
            _headerReader = new HeaderReader();
            _dataReader = new DataReader();
        }

        public Reader(string edfFilePath)
            : this(GetStreamFromFilePath(edfFilePath)) { }

        ~Reader()
        {
            Dispose(false); // Clean up the unmanaged resources.
        }

        private static Stream GetStreamFromFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException($"{nameof(filePath)} is null or empty!", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file at path {filePath} was not found!", filePath);

            return File.OpenRead(filePath);
        }

        public HeaderRecord ReadHeader()
        {
            _headerRecord = _headerReader.Read(_binaryReader);
            return _headerRecord;
        }

        public double[] ReaderData(int signal, int count)
        {
            if (_headerRecord == null)
                ReadHeader();

            return _dataReader.Read(_binaryReader, _headerRecord, signal, count);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // 垃圾回收期可能让_binaryReader释放了，所以无法调用成功（也就是无法做到
                // 十全十美）
                // Dispose managed
                _binaryReader.Dispose();
            }

            // Dispose unmanaged
            if (_stream != null)
                _stream.Dispose();

            _disposed = true;
        }
    }
}
