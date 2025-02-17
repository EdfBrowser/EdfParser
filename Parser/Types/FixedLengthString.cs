namespace Parser
{
    internal class FixedLengthString : Item
    {
        internal FixedLengthString(Field info) : base(info) { }

        internal string Value { get; set; }

        protected override string ToAscii()
        {
            string asciiString = "";
            if (Value != null)
                asciiString = Value.PadRight(AsciiLength, ' ');
            else
                asciiString = asciiString.PadRight(AsciiLength, ' ');

            return asciiString;
        }
    }
}
