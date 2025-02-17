namespace Parser
{
    internal class VariableLengthInt : Item
    {
        internal int[] Value { get; set; }
        internal VariableLengthInt(Field info) : base(info) { }

        protected override string ToAscii()
        {
            string ascii = "";
            foreach (var intVal in Value)
            {
                string temp = intVal.ToString();
                if (temp.Length > AsciiLength)
                    temp = temp.Substring(0, AsciiLength);
                ascii += temp;
            }
            return ascii;
        }
    }
}
