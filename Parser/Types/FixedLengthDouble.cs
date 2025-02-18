using System;
using System.IO;
using System.Text;

namespace Parser
{
    internal class FixedLengthDouble : Item
    {
        internal double Value { get; set; }
        internal FixedLengthDouble(Field info) : base(info) { }

        internal override void Read(BinaryReader reader, int ns = 1)
        {
            byte[] bytes = reader.ReadBytes(Length);
            // 将byte数组的值转换成对应的字符
            string str = Encoding.ASCII.GetString(bytes);
            Value = Convert.ToDouble(str);
        }

        protected override string ToAscii()
        {
            string asciiString = "";
            if (Value != null)
            {
                asciiString = Value.ToString();
                if (asciiString.Length >= Length)
                    asciiString = asciiString.Substring(0, Length);
                else
                    asciiString = Value.ToString().PadRight(Length, ' ');
            }

            else
                asciiString = asciiString.PadRight(Length, ' ');

            return asciiString;
        }
    }
}
