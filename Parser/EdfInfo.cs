using System;

namespace Parser
{
    public readonly struct EdfInfo
    {
        public EdfInfo(DateTime dt, int dataRecords,
            int durationForDataRecord, int signals, SignalInfo[] signalInfos)
        {
            DateTime = dt;
            DataRecords = dataRecords;
            DurationForDataRecord = durationForDataRecord;
            Signals = signals;
            SignalInfos = signalInfos;
        }

        public DateTime DateTime { get; }
        public int DataRecords { get; }
        public int DurationForDataRecord { get; }
        public int Signals { get; }

        public SignalInfo[] SignalInfos { get; }
    }

    public readonly struct SignalInfo
    {
        public SignalInfo(int channel, string label, string dimension,
            string prefiltering, int samples)
        {
            Channel = channel;
            Label = label;
            PhysicalDimension = dimension;
            Prefiltering = prefiltering;
            Samples = samples;
        }

        public int Channel { get; }
        public string Label { get; }
        public string PhysicalDimension { get; }
        public string Prefiltering { get; }
        public int Samples { get; }
    }
}
