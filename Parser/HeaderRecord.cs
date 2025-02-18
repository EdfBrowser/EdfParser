using System.IO;

namespace Parser
{
    public class HeaderRecord
    {
        private static class HeaderItems
        {
            static HeaderItems()
            {
                Version = new Field("Version", 8);
                PatientID = new Field("PatientID", 80);
                RecordID = new Field("RecordID", 80);
                StartDate = new Field("StartDate", 8);
                StartTime = new Field("StartTime", 8);
                NumberOfBytesInHeader = new Field("NumberOfBytesInHeader", 8);
                Reserved = new Field("Reserved", 44);
                NumberOfDataRecords = new Field("NumberOfDataRecords", 8);
                DurationOfDataRecord = new Field("DurationOfDataRecord", 8);
                NumberOfSignals = new Field("NumberOfSignals", 4);

                Label = new Field("Label", 16);
                TransducerType = new Field("TransducerType", 80);
                PhysicalDimension = new Field("PhysicalDimension", 8);
                PhysicalMinimum = new Field("PhysicalMinimum", 8);
                PhysicalMaximum = new Field("PhysicalMaximum", 8);
                DigitalMinimum = new Field("DigitalMinimum", 8);
                DigitalMaximum = new Field("DigitalMaximum", 8);
                Prefiltering = new Field("Prefiltering", 80);
                NumberOfSamplesInDataRecord = new Field("NumberOfSamplesInDataRecord", 8);
                SignalsReserved = new Field("SignalsReserved", 32);
            }

            // Fixed length
            internal static Field Version { get; }
            internal static Field PatientID { get; }
            internal static Field RecordID { get; }
            internal static Field StartDate { get; }
            internal static Field StartTime { get; }
            internal static Field NumberOfBytesInHeader { get; }
            internal static Field Reserved { get; }
            internal static Field NumberOfDataRecords { get; }
            internal static Field DurationOfDataRecord { get; }
            internal static Field NumberOfSignals { get; }

            // Mutable length
            internal static Field Label { get; }
            internal static Field TransducerType { get; }
            internal static Field PhysicalDimension { get; }
            internal static Field PhysicalMinimum { get; }
            internal static Field PhysicalMaximum { get; }
            internal static Field DigitalMinimum { get; }
            internal static Field DigitalMaximum { get; }
            internal static Field Prefiltering { get; }
            internal static Field NumberOfSamplesInDataRecord { get; }
            internal static Field SignalsReserved { get; }
        }

        internal HeaderRecord()
        {
            Version = new FixedLengthString(HeaderItems.Version);
            PatientID = new FixedLengthString(HeaderItems.PatientID);
            RecordID = new FixedLengthString(HeaderItems.RecordID);
            StartDate = new FixedLengthString(HeaderItems.StartDate);
            StartTime = new FixedLengthString(HeaderItems.StartTime);
            NumberOfBytesInHeader = new FixedLengthInt(HeaderItems.NumberOfBytesInHeader);
            Reserved = new FixedLengthString(HeaderItems.Reserved);
            NumberOfDataRecords = new FixedLengthInt(HeaderItems.NumberOfDataRecords);
            DurationOfDataRecord = new FixedLengthInt(HeaderItems.DurationOfDataRecord);
            NumberOfSignals = new FixedLengthInt(HeaderItems.NumberOfSignals);

            Labels = new VariableLengthString(HeaderItems.Label);
            TransducerType = new VariableLengthString(HeaderItems.TransducerType);
            PhysicalDimension = new VariableLengthString(HeaderItems.PhysicalDimension);
            PhysicalMinimum = new VariableLengthDouble(HeaderItems.PhysicalMinimum);
            PhysicalMaximum = new VariableLengthDouble(HeaderItems.PhysicalMaximum);
            DigitalMinimum = new VariableLengthInt(HeaderItems.DigitalMinimum);
            DigitalMaximum = new VariableLengthInt(HeaderItems.DigitalMaximum);
            Prefiltering = new VariableLengthString(HeaderItems.Prefiltering);
            NumberOfSamplesInDataRecord = new VariableLengthInt(HeaderItems.NumberOfSamplesInDataRecord);
            SignalsReserved = new VariableLengthString(HeaderItems.SignalsReserved);

            RecordSize = 0L;

            const int MAXSIGNALS = 1000;
            SamplePos = new int[MAXSIGNALS];
            Annotation = new bool[MAXSIGNALS];
            MappedSignals = new int[MAXSIGNALS];
            BufOffset = new long[MAXSIGNALS];
            BitValues = new double[MAXSIGNALS];
            Offsets = new double[MAXSIGNALS];
        }

        internal FixedLengthString Version { get; }
        internal FixedLengthString PatientID { get; }
        internal FixedLengthString RecordID { get; }
        internal FixedLengthString StartDate { get; }
        internal FixedLengthString StartTime { get; }
        internal FixedLengthInt NumberOfBytesInHeader { get; }
        internal FixedLengthString Reserved { get; }
        internal FixedLengthInt NumberOfDataRecords { get; }
        internal FixedLengthInt DurationOfDataRecord { get; }
        internal FixedLengthInt NumberOfSignals { get; }

        internal VariableLengthString Labels { get; }
        internal VariableLengthString TransducerType { get; }
        internal VariableLengthString PhysicalDimension { get; }
        internal VariableLengthDouble PhysicalMinimum { get; }
        internal VariableLengthDouble PhysicalMaximum { get; }
        internal VariableLengthInt DigitalMinimum { get; }
        internal VariableLengthInt DigitalMaximum { get; }
        internal VariableLengthString Prefiltering { get; }
        internal VariableLengthInt NumberOfSamplesInDataRecord { get; }
        internal VariableLengthString SignalsReserved { get; }

        // TODO: 重构_signalsMetadata 
        internal int[] SamplePos { get; set; }
        internal bool[] Annotation { get; set; }
        internal int[] MappedSignals { get; set; }
        // 一个数据块字节数
        internal long RecordSize { get; set; }
        internal long[] BufOffset { get; set; }
        internal double[] BitValues { get; set; }
        internal double[] Offsets { get; set; }


        internal void Read(BinaryReader reader)
        {
            Version.Read(reader);
            PatientID.Read(reader);
            RecordID.Read(reader);
            StartDate.Read(reader);
            StartTime.Read(reader);
            NumberOfBytesInHeader.Read(reader);
            Reserved.Read(reader);
            NumberOfDataRecords.Read(reader);
            DurationOfDataRecord.Read(reader);
            NumberOfSignals.Read(reader);


            int ns = NumberOfSignals.Value;
            Labels.Read(reader, ns);
            TransducerType.Read(reader, ns);
            PhysicalDimension.Read(reader, ns);
            PhysicalMinimum.Read(reader, ns);
            PhysicalMaximum.Read(reader, ns);
            DigitalMinimum.Read(reader, ns);
            DigitalMaximum.Read(reader, ns);
            Prefiltering.Read(reader, ns);
            NumberOfSamplesInDataRecord.Read(reader, ns);
            SignalsReserved.Read(reader, ns);


            //
            int j = 0;
            for (int i = 0; i < NumberOfSignals.Value; i++)
            {
                if (!Annotation[i])
                    MappedSignals[j++] = i;
            }

            for (int i = 0; i < NumberOfSignals.Value; i++)
            {
                RecordSize += NumberOfSamplesInDataRecord.Value[i];
            }
            RecordSize *= 2;

            long n = 0L;
            for (int i = 0; i < NumberOfSignals.Value; i++)
            {
                BufOffset[i] = n;
                n += NumberOfSamplesInDataRecord.Value[i] * 2;
            }

            for (int i = 0; i < NumberOfSignals.Value; i++)
            {
                double physicalMax = PhysicalMaximum.Value[i];
                double physicalMin = PhysicalMinimum.Value[i];

                double digitalMax = DigitalMaximum.Value[i];
                double digitalMin = DigitalMinimum.Value[i];

                BitValues[i] = (physicalMax - physicalMin) / (digitalMax - digitalMin);
                Offsets[i] = physicalMax / BitValues[i] - digitalMax;
            }

        }

        public override string ToString()
        {
            string strOutput = "";

            strOutput += "\tVersion [" + Version.Value + "]\n";
            strOutput += "\tPatient ID [" + PatientID.Value + "]\n";
            strOutput += "\tRecording ID [" + RecordID.Value + "]\n";
            strOutput += "\tStart Date [" + StartDate.Value + "]\n";
            strOutput += "\tStart Time [" + StartTime.Value + "]\n";
            strOutput += "\tNumber of bytes in header [" + NumberOfBytesInHeader.Value + "]\n";
            strOutput += "\tReserved [" + Reserved.Value + "]\n";
            strOutput += "\tNumber of data records [" + NumberOfDataRecords.Value + "]\n";
            strOutput += "\tDuration of data record [" + DurationOfDataRecord.Value + "]\n";
            strOutput += "\tNumber of signals [" + NumberOfSignals.Value + "]\n";

            for (int i = 0; i < NumberOfSignals.Value; i++)
            {
                strOutput += "\tLabels [" + Labels.Value[i] + "]\n";
                strOutput += "\tTransducer type [" + TransducerType.Value[i] + "]\n";
                strOutput += "\tPhysical dimension [" + PhysicalDimension.Value[i] + "]\n";
                strOutput += "\tPhysical minimum [" + PhysicalMinimum.Value[i] + "]\n";
                strOutput += "\tPhysical maximum [" + PhysicalMaximum.Value[i] + "]\n";
                strOutput += "\tDigital minimum [" + DigitalMinimum.Value[i] + "]\n";
                strOutput += "\tDigital maximum [" + DigitalMaximum.Value[i] + "]\n";
                strOutput += "\tPrefiltering [" + Prefiltering.Value[i] + "]\n";
                strOutput += "\tNumber of samples in data record [" + NumberOfSamplesInDataRecord.Value[i] + "]\n";
                strOutput += "\tSignals reserved [" + SignalsReserved.Value[i] + "]\n";
            }

            return strOutput;
        }
    }
}
