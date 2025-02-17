namespace Parser
{
    internal class VariableLengthString : Item
    {
        internal string[] Value { get; set; }
        internal VariableLengthString(Field info) : base(info) { }

        protected override string ToAscii()
        {
            string ascii = "";
            foreach (var strVal in Value)
            {
                string temp = strVal.ToString();
                if (strVal.Length > AsciiLength)
                    temp = temp.Substring(0, AsciiLength);
                ascii += temp;
            }
            return ascii;
        }
    }
}
