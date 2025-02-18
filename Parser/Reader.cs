using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
        private readonly HeaderRecord _headerRecord;
        private readonly BinaryReader _binaryReader;

        internal DataReader(HeaderRecord headerRecord, BinaryReader binaryReader)
        {
            _headerRecord = headerRecord;
            _binaryReader = binaryReader;
        }

        private const int _bytesPerSmp = 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long CalculateOffset(int channel)
        {
            int ns = _headerRecord.NumberOfSignals.Value;
            int smpPos = _headerRecord.SamplePos[channel];
            int smpPerRecord = _headerRecord.NumberOfSamplesInDataRecord.Value[channel];
            long recordSize = _headerRecord.RecordSize;
            long bufOffset = _headerRecord.BufOffset[channel];

            // 头部分大小
            long offset = 256 + ns * 256;
            // 之前读取到第几块了
            offset += (smpPos / smpPerRecord) * recordSize;
            // 该channel在块中偏移多少字节
            offset += bufOffset;
            // 从当前块开始偏移了多少字节
            offset += (smpPos % smpPerRecord) * _bytesPerSmp;

            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AdjustCount(int channel, int count)
        {
            int smpPos = _headerRecord.SamplePos[channel];
            int smpPerRecord = _headerRecord.NumberOfSamplesInDataRecord.Value[channel];
            int smpInFile = smpPerRecord * _headerRecord.NumberOfDataRecords.Value;

            // 如果 count 大于文件内剩余的样本数，则调整 count 为文件剩余的样本数
            if ((smpPos + count) > smpInFile)
            {
                count = smpInFile - smpPos;
                if (count == 0)
                    return 0;

                if (count < 0)
                    throw new ArgumentException("Invalid calculation: the count value became negative.");
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculatePhysicalValue(int channel, short rawVal)
        {
            double bitVal = _headerRecord.BitValues[channel];
            double offsetVal = _headerRecord.Offsets[channel];

            return bitVal * (rawVal + offsetVal);
        }

        // 方式二
        //while (currentIndex < count)
        //{
        //    // 当前信号的数据块的剩余样本数
        //    int remainingSamplesInBlock = smpPerRecord - (smpPos % smpPerRecord);
        //    // 当前批次要读取的样本数：当前块剩余样本数与所需样本数取较小值
        //    int batchCount = Math.Min(remainingSamplesInBlock, count - currentIndex);

        //    //byte[] dataBatch = _reader.ReadBytes(batchCount * bytesPerSample);

        //    //// 处理读取的数据
        //    //for (int i = 0; i < batchCount; i++)
        //    //{
        //    //    int byteIndex = i * bytesPerSample; // 计算当前样本的字节索引
        //    //    short rawSample = BitConverter.ToInt16(dataBatch, byteIndex); // 从指定位置解析样本
        //    //    result[currentIndex++] = _headerRecord.BitValues[channel] * (rawSample + _headerRecord.Offsets[channel]);
        //    //}

        //    // 更新样本位置
        //    smpPos += batchCount;
        //    currentIndex += batchCount;

        //    //// 如果当前块已经读取完，需要跳到下一块的开始位置
        //    //if (smpPos % smpPerRecord == 0 && currentIndex < count)
        //    //{
        //    //    long jump = _headerRecord.RecordSize - (smpPerRecord * bytesPerSample);
        //    //    _reader.BaseStream.Seek(jump, SeekOrigin.Current); // 跳过当前块的剩余部分
        //    //}
        //}
        internal async Task<int> ReadCore(int signal, double[] buf)
        {
            int ns = _headerRecord.NumberOfSignals.Value;
            int count = buf.Length;

            if (signal < 0 || signal >= ns)
                throw new ArgumentException($"The {nameof(signal)} value is out of the valid range!");

            if (count < 0)
                throw new ArgumentException($"The {nameof(count)} value is invalid!");

            if (count == 0)
                return 0;

            int channel = _headerRecord.MappedSignals[signal];
            int smpPos = _headerRecord.SamplePos[channel];
            int smpPerRecord = _headerRecord.NumberOfSamplesInDataRecord.Value[channel];

            long offset = CalculateOffset(channel);
            _binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            count = AdjustCount(channel, count);
            if (count == 0)
                return 0;

            int BATCHSIZE = smpPerRecord;
            int numBatches = (int)Math.Ceiling((double)count / BATCHSIZE);

            int currentIndex = 0;

            // 假如当前的位置是dataRecord的末尾，需要跳到下一块的开始
            long jump = _headerRecord.RecordSize - (smpPerRecord * _bytesPerSmp);

            for (int i = 0; i < numBatches; i++)
            {
                if (smpPos % smpPerRecord == 0 && i > 0)
                    _binaryReader.BaseStream.Seek(jump, SeekOrigin.Current);

                // 计算当前批次要读取的样本数
                int batchCount = Math.Min(BATCHSIZE, count - currentIndex);
                byte[] dataBatch = new byte[batchCount * _bytesPerSmp];
                await _binaryReader.BaseStream.ReadAsync(dataBatch, 0, batchCount * _bytesPerSmp);

                // 处理读取的数据
                for (int j = 0; j < batchCount; j++)
                {
                    short rawSample = BitConverter.ToInt16(dataBatch, j * _bytesPerSmp);
                    buf[currentIndex++] = CalculatePhysicalValue(channel, rawSample);
                }

                smpPos += batchCount;
            }

            int oldPos = _headerRecord.SamplePos[channel];
            _headerRecord.SamplePos[channel] = smpPos;

            return smpPos - oldPos;
        }
    }

    public class Reader : IDisposable
    {
        private readonly Stream _stream;
        private readonly BinaryReader _binaryReader;
        private readonly HeaderReader _headerReader;
        private readonly DataReader _dataReader;

        private readonly HeaderRecord _headerRecord;
        private bool _disposed;

        public Reader(Stream stream)
        {
            _disposed = false;

            _stream = stream
                ?? throw new ArgumentNullException($"{nameof(stream)} is null!");
            _binaryReader = new BinaryReader(stream);
            _headerReader = new HeaderReader();
            _headerRecord = _headerReader.Read(_binaryReader);
            _dataReader = new DataReader(_headerRecord, _binaryReader);
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

        public async Task<int> ReadDataAsync(int signal, double[] buf)
            => await _dataReader.ReadCore(signal, buf).ConfigureAwait(false);

        //public double[] ReadFromMemoryMappingFile(string file, int signal, int count)
        //{
        //    Dispose();

        //    int ns = _headerRecord.NumberOfSignals.Value;

        //    if (signal < 0 || signal >= ns)
        //        throw new ArgumentException($"The {nameof(signal)} value is out of the valid range!");

        //    if (count < 0)
        //        throw new ArgumentException($"The {nameof(count)} value is invalid!");

        //    if (count == 0)
        //        return Array.Empty<double>();

        //    int bytesPerSmp = 2;

        //    int channel = _headerRecord.MappedSignals[signal];
        //    int smpPos = _headerRecord.SamplePos[channel];
        //    int smpPerRecord = _headerRecord.NumberOfSamplesInDataRecord.Value[channel];
        //    int smpInFile = smpPerRecord * _headerRecord.NumberOfDataRecords.Value;

        //    // 如果 count 大于文件内剩余的样本数，则调整 count 为文件剩余的样本数
        //    if ((smpPos + count) > smpInFile)
        //    {
        //        count = smpInFile - smpPos;
        //        if (count == 0)
        //            return Array.Empty<double>();

        //        if (count < 0)
        //            throw new ArgumentException("Invalid calculation: the count value became negative.");
        //    }


        //    int BATCHSIZE = smpPerRecord;
        //    int numBatches = (int)Math.Ceiling((double)count / BATCHSIZE);

        //    double[] buf = new double[count];
        //    int currentIndex = 0;

        //    // 头部分大小
        //    long offset = 256 + ns * 256;
        //    // 之前读取到第几块了
        //    offset += (smpPos / smpPerRecord) * _headerRecord.RecordSize;
        //    // 该channel在块中偏移多少字节
        //    offset += _headerRecord.BufOffset[channel];
        //    // 从当前块开始偏移了多少字节
        //    offset += (smpPos % smpPerRecord) * bytesPerSmp;

        //    var mmf = MemoryMappedFile.CreateFromFile(file, FileMode.Open);

        //    // 假如当前的位置是dataRecord的末尾，需要跳到下一块的开始
        //    long jump = _headerRecord.RecordSize - (smpPerRecord * bytesPerSmp);

        //    for (int i = 0; i < numBatches; i++)
        //    {
        //        // 每次读取一个批次的数据
        //        int batchCount = Math.Min(BATCHSIZE, count - currentIndex);

        //        if (smpPos % smpPerRecord == 0 && i > 0)
        //            offset += jump;

        //        using (var accessor = mmf.CreateViewAccessor(offset, batchCount * bytesPerSmp))
        //        {
        //            for (int j = 0; j < batchCount; j++)
        //            {
        //                short rawSample = accessor.ReadInt16(j * bytesPerSmp);

        //                buf[currentIndex++] = _headerRecord.BitValues[channel] * (rawSample + _headerRecord.Offsets[channel]);
        //            }
        //        }

        //        smpPos += batchCount;
        //        offset += batchCount * bytesPerSmp;
        //    }

        //    _headerRecord.SamplePos[channel] = smpPos;

        //    mmf.Dispose();

        //    return buf;
        //}

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
