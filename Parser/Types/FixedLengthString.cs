using System.IO;
using System.Text;

namespace Parser
{
    internal class FixedLengthString : Item
    {
        internal FixedLengthString(Field info) : base(info) { }

        internal string Value { get; set; }

        internal override void Read(BinaryReader reader, int ns = 1)
        {
            byte[] bytes = reader.ReadBytes(Length);
            Value = Encoding.ASCII.GetString(bytes);
        }

        protected override string ToAscii()
        {
            string asciiString = "";
            if (Value != null)
                asciiString = Value.PadRight(Length, ' ');
            else
                asciiString = asciiString.PadRight(Length, ' ');

            return asciiString;
        }
    }
}
