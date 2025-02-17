using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parser
{
    internal class Reader : BinaryReader
    {
        private HeaderRecord _curHeadRecord;

        internal Reader(Stream stream) : base(stream) { }

        internal HeaderRecord ReadHeaderRecord()
        {
            HeaderRecord h = new HeaderRecord();

            //--------------Fixed length------------
            h.Version.Value = ReadAscii(h.Version.AsciiLength);
            h.PatientID.Value = ReadAscii(h.PatientID.AsciiLength);
            h.RecordID.Value = ReadAscii(h.RecordID.AsciiLength);
            h.StartDate.Value = ReadAscii(h.StartDate.AsciiLength);
            h.StartTime.Value = ReadAscii(h.StartTime.AsciiLength);
            h.NumberOfBytesInHeader.Value = ReadInt(h.NumberOfBytesInHeader.AsciiLength);
            h.Reserved.Value = ReadAscii(h.Reserved.AsciiLength);
            h.NumberOfDataRecords.Value = ReadInt(h.NumberOfDataRecords.AsciiLength);
            h.DurationOfDataRecord.Value = ReadInt(h.DurationOfDataRecord.AsciiLength);
            h.NumberOfSignals.Value = ReadInt(h.NumberOfSignals.AsciiLength);

            //--------------Mutable length-----------
            int ns = h.NumberOfSignals.Value;
            h.Labels.Value = ReadMultipleAscii(h.Labels.AsciiLength, ns);
            h.TransducerType.Value = ReadMultipleAscii(h.TransducerType.AsciiLength, ns);
            h.PhysicalDimension.Value = ReadMultipleAscii(h.PhysicalDimension.AsciiLength, ns);
            h.PhysicalMinimum.Value = ReadMultipleDouble(h.PhysicalMinimum.AsciiLength, ns);
            h.PhysicalMaximum.Value = ReadMultipleDouble(h.PhysicalMaximum.AsciiLength, ns);
            h.DigitalMinimum.Value = ReadMultipleInt(h.DigitalMinimum.AsciiLength, ns);
            h.DigitalMaximum.Value = ReadMultipleInt(h.DigitalMaximum.AsciiLength, ns);
            h.Prefiltering.Value = ReadMultipleAscii(h.Prefiltering.AsciiLength, ns);
            h.NumberOfSamplesInDataRecord.Value = ReadMultipleInt(h.NumberOfSamplesInDataRecord.AsciiLength, ns);
            h.SignalsReserved.Value = ReadMultipleAscii(h.SignalsReserved.AsciiLength, ns);

            //
            int j = 0;
            for (int i = 0; i < h.NumberOfSignals.Value; i++)
            {
                if (!h.Annotation[i])
                    h.MappedSignals[j++] = i;
            }

            for (int i = 0; i < h.NumberOfSignals.Value; i++)
            {
                h.RecordSize += h.NumberOfSamplesInDataRecord.Value[i];
            }
            h.RecordSize *= 2;

            long n = 0L;
            for (int i = 0; i < h.NumberOfSignals.Value; i++)
            {
                h.BufOffset[i] = n;
                n += h.NumberOfSamplesInDataRecord.Value[i] * 2;
            }

            for (int i = 0; i < h.NumberOfSignals.Value; i++)
            {
                double physicalMax = h.PhysicalMaximum.Value[i];
                double physicalMin = h.PhysicalMinimum.Value[i];

                double digitalMax = h.DigitalMaximum.Value[i];
                double digitalMin = h.DigitalMinimum.Value[i];

                h.BitValues[i] = (physicalMax - physicalMin) / (digitalMax - digitalMin);
                h.Offsets[i] = physicalMax / h.BitValues[i] - digitalMax;
            }

            _curHeadRecord = h;

            return h;
        }


        public double[] ReadPhsyicalSamples(int signal, long count)
        {
            if (signal < 0 || signal >= _curHeadRecord.NumberOfSignals.Value)
                throw new ArgumentException($"The {nameof(signal)} value is out of the valid range!");

            if (count < 0)
                throw new ArgumentException($"The {nameof(count)} value is invalid!");

            if (count == 0)
                return Array.Empty<double>();

            int channel = _curHeadRecord.MappedSignals[signal];

            int bytesPerSmp = 2;

            int smpInFile = _curHeadRecord.NumberOfSamplesInDataRecord.Value[channel]
                * _curHeadRecord.NumberOfDataRecords.Value;
            if ((_curHeadRecord.SamplePos[channel] + count) > smpInFile)
            {
                count = smpInFile - _curHeadRecord.SamplePos[channel];
                if (count == 0)
                    return Array.Empty<double>();

                if (count < 0)
                    throw new ArgumentException("Invalid calculation: the count value became negative.");
            }

            double[] buf = new double[count];

            long offset = 256 + _curHeadRecord.NumberOfSignals.Value * 256;

            offset += (_curHeadRecord.SamplePos[channel] / _curHeadRecord.NumberOfSamplesInDataRecord.Value[channel])
                * _curHeadRecord.RecordSize;

            offset += _curHeadRecord.BufOffset[channel];

            offset += (_curHeadRecord.SamplePos[channel] % _curHeadRecord.NumberOfSamplesInDataRecord.Value[channel])
                * bytesPerSmp;

            BaseStream.Seek(offset, SeekOrigin.Begin);

            long smpPos = _curHeadRecord.SamplePos[channel];
            int smpPerRecord = _curHeadRecord.NumberOfSamplesInDataRecord.Value[channel];
            // 假如当前的位置是dataRecord的末尾，需要跳到下一块的开始
            long jump = _curHeadRecord.RecordSize - (smpPerRecord * bytesPerSmp);

            for (int i = 0; i < count; i++)
            {
                if (smpPos % smpPerRecord == 0)
                {
                    if (i > 0)
                    {
                        BaseStream.Seek(jump, SeekOrigin.Current);
                    }
                }

                byte one = ReadByte();
                if (PeekChar() == -1)
                {
                    throw new ArgumentException("The stream is on the end");
                }

                byte two = ReadByte();
                double val = BitConverter.ToInt16(new byte[] { one, two }, 0);

                buf[i] = _curHeadRecord.BitValues[channel] * (val + _curHeadRecord.Offsets[channel]);

                smpPos++;
            }

            _curHeadRecord.SamplePos[channel] = smpPos;

            return buf;
        }


        private int ReadInt(int asciiLength)
        {
            string strInt = ReadAscii(asciiLength).Trim();
            int intResult = -1;

            try
            {
                intResult = Convert.ToInt32(strInt);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn`t convert string to integer. " + ex.Message);
            }

            return intResult;
        }

        private string ReadAscii(int asciiLength)
        {
            byte[] bytes = ReadBytes(asciiLength);
            return AsciiString(bytes);
        }

        private string[] ReadMultipleAscii(int asciiLength, int ns)
        {
            var parts = new List<string>();

            for (int i = 0; i < ns; i++)
            {
                byte[] bytes = ReadBytes(asciiLength);
                parts.Add(AsciiString(bytes));
            }

            return parts.ToArray();
        }

        private int[] ReadMultipleInt(int asciiLength, int ns)
        {
            var parts = new List<int>();

            for (int i = 0; i < ns; i++)
            {
                byte[] bytes = ReadBytes(asciiLength);
                string ascii = AsciiString(bytes);
                parts.Add(Convert.ToInt32(ascii));
            }

            return parts.ToArray();
        }

        private double[] ReadMultipleDouble(int asciiLength, int ns)
        {
            var parts = new List<double>();

            for (int i = 0; i < ns; i++)
            {
                byte[] bytes = this.ReadBytes(asciiLength);
                string ascii = AsciiString(bytes);
                parts.Add(Convert.ToDouble(ascii));
            }

            return parts.ToArray();
        }

        private string AsciiString(byte[] bytes) => Encoding.ASCII.GetString(bytes);
    }
}
