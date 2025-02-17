namespace Parser
{
    internal class FixedLengthInt : Item
    {
        internal int Value { get; set; }
        internal FixedLengthInt(Field info) : base(info) { }

        protected override string ToAscii()
        {
            string asciiString = "";
            if (Value != null)
                asciiString = Value.ToString().PadRight(AsciiLength, ' ');
            else
                asciiString = asciiString.PadRight(AsciiLength, ' ');

            return asciiString;
        }
    }
}
