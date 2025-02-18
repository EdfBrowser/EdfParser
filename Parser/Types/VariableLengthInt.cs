using System;
using System.IO;
using System.Text;

namespace Parser
{
    internal class VariableLengthInt : Item
    {
        internal int[] Value { get; set; }
        internal VariableLengthInt(Field info) : base(info) { }

        internal override void Read(BinaryReader reader, int ns = 1)
        {
            Value = new int[ns];
            for (int i = 0; i < ns; i++)
            {
                byte[] bytes = reader.ReadBytes(Length);
                // 将byte数组的值转换成对应的字符
                string str = Encoding.ASCII.GetString(bytes);
                Value[i] = Convert.ToInt32(str);
            }
        }

        protected override string ToAscii()
        {
            string ascii = "";
            foreach (var intVal in Value)
            {
                string temp = intVal.ToString();
                if (temp.Length > Length)
                    temp = temp.Substring(0, Length);
                ascii += temp;
            }
            return ascii;
        }
    }
}
