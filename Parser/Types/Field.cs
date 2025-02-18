namespace Parser
{
    internal readonly struct Field
    {
        private readonly string _name;
        private readonly int _asciiLength;

        internal Field(string name, int asciiLength)
        {
            _name = name;
            _asciiLength = asciiLength;
        }

        internal string Name => _name;
        internal int AsciiLength => _asciiLength;
    }
}
