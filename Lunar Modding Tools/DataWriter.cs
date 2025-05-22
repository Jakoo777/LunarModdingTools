using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lunar_Modding_Tools
{
    public class DataWriter : BinaryWriter
    {
        public bool IsLittleEndian { get; set; } = true;

        public DataWriter()
            : base(new MemoryStream())
        {
        }

        public DataWriter(Stream stream, bool littleEndian = true)
            : base(stream)
        {
            IsLittleEndian = littleEndian;
        }

        public override void Write(short value)
        {
            var data = BitConverter.GetBytes(value);
            if (!IsLittleEndian)
                Array.Reverse(data);
            base.Write(data);
        }

        public override void Write(ushort value)
        {
            var data = BitConverter.GetBytes(value);
            if (!IsLittleEndian)
                Array.Reverse(data);
            base.Write(data);
        }

        public override void Write(int value)
        {
            var data = BitConverter.GetBytes(value);
            if (!IsLittleEndian)
                Array.Reverse(data);
            base.Write(data);
        }

        public override void Write(uint value)
        {
            var data = BitConverter.GetBytes(value);
            if (!IsLittleEndian)
                Array.Reverse(data);
            base.Write(data);
        }

        public void WriteFixedString(string text, int length, Encoding encoding = null)
        {
            encoding ??= Encoding.ASCII;
            var data = encoding.GetBytes(text);
            Array.Resize(ref data, length);
            base.Write(data);
        }

        public void WriteRGB555(System.Drawing.Color color)
        {
            ushort val =
                (ushort)(((color.B >> 3) << 10) |
                         ((color.G >> 3) << 5) |
                         ((color.R >> 3)));
            Write(val);
        }


        public byte[] ToArray()
        {
            var ms = (MemoryStream)BaseStream;
            return ms.ToArray();
        }
    }
}
