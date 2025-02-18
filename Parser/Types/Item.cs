using System.IO;

namespace Parser
{
    internal abstract class Item
    {
        private readonly string _name;
        private readonly int _length;

        protected Item(Field info)
        {
            _name = info.Name;
            _length = info.AsciiLength;
        }

        internal string Name => _name;
        internal int Length => _length;

        internal abstract void Read(BinaryReader reader, int ns = 1);
        protected abstract string ToAscii();
    }
}
