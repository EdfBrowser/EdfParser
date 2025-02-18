using System.IO;
using System.Text;

namespace Parser
{
    internal class VariableLengthString : Item
    {
        internal string[] Value { get; set; }
        internal VariableLengthString(Field info) : base(info) { }

        internal override void Read(BinaryReader reader, int ns = 1)
        {
            Value = new string[ns];
            for (int i = 0; i < ns; i++)
            {
                byte[] bytes = reader.ReadBytes(Length);
                // 将byte数组的值转换成对应的字符
                string str = Encoding.ASCII.GetString(bytes);
                Value[i] = str;
            }
        }

        protected override string ToAscii()
        {
            string ascii = "";
            foreach (var strVal in Value)
            {
                string temp = strVal.ToString();
                if (strVal.Length > Length)
                    temp = temp.Substring(0, Length);
                ascii += temp;
            }
            return ascii;
        }
    }
}
