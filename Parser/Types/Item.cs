namespace Parser
{
    internal abstract class Item
    {
        private readonly string _name;
        private readonly int _asciiLength;

        protected Item(Field info)
        {
            _name = info.Name;
            _asciiLength = info.AsciiLength;
        }

        internal string Name => _name;
        internal int AsciiLength => _asciiLength;
        protected abstract string ToAscii();
    }
}
