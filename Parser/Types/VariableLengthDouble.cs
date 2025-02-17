namespace Parser
{
    internal class VariableLengthDouble : Item
    {
        internal double[] Value { get; set; }
        internal VariableLengthDouble(Field info) : base(info) { }

        protected override string ToAscii()
        {
            string ascii = "";
            foreach (var doubleVal in Value)
            {
                string temp = doubleVal.ToString();
                if (temp.Length > AsciiLength)
                    temp = temp.Substring(0, AsciiLength);
                ascii += temp;
            }
            return ascii;
        }
    }
}
