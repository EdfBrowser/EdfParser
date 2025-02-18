using System;
using System.IO;
using System.Text;

namespace Parser
{
    internal class VariableLengthDouble : Item
    {
        internal double[] Value { get; set; }
        internal VariableLengthDouble(Field info) : base(info) { }

        internal override void Read(BinaryReader reader, int ns = 1)
        {
            Value = new double[ns];
            for (int i = 0; i < ns; i++)
            {
                byte[] bytes = reader.ReadBytes(Length);
                // 将byte数组的值转换成对应的字符
                string str = Encoding.ASCII.GetString(bytes);
                Value[i] = Convert.ToDouble(str);
            }
        }

        protected override string ToAscii()
        {
            string ascii = "";
            foreach (var doubleVal in Value)
            {
                string temp = doubleVal.ToString();
                if (temp.Length > Length)
                    temp = temp.Substring(0, Length);
                ascii += temp;
            }
            return ascii;
        }
    }
}
