namespace Parser
{
    internal class FixedLengthDouble : Item
    {
        internal double Value { get; set; }
        internal FixedLengthDouble(Field info) : base(info) { }

        protected override string ToAscii()
        {
            string asciiString = "";
            if (Value != null)
            {
                asciiString = Value.ToString();
                if (asciiString.Length >= AsciiLength)
                    asciiString = asciiString.Substring(0, AsciiLength);
                else
                    asciiString = Value.ToString().PadRight(AsciiLength, ' ');
            }

            else
                asciiString = asciiString.PadRight(AsciiLength, ' ');

            return asciiString;
        }
    }
}
