using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lunar_Modding_Tools
{
    public class DataReader : BinaryReader
    {
        public bool IsLittleEndian { get; set; } = true;

        public DataReader(Stream s) : base(s)
        {
        }

        public DataReader(byte[] bytes) : base(new MemoryStream(bytes))
        {
        }

        public DataReader(byte[] bytes, uint position, uint size)
            : base(new MemoryStream(bytes, (int)position, (int)size))
        {
        }

        public DataReader(Stream stream, bool littleEndian = true)
            : base(stream)
        {
            IsLittleEndian = littleEndian;
        }

        public DataReader(byte[] bytes, bool littleEndian = true)
            : base(new MemoryStream(bytes))
        {
            IsLittleEndian = littleEndian;
        }

        public override short ReadInt16()
        {
            var data = base.ReadBytes(2);
            if (!IsLittleEndian)
                Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public override ushort ReadUInt16()
        {
            var data = base.ReadBytes(2);
            if (!IsLittleEndian)
                Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public override int ReadInt32()
        {
            var data = base.ReadBytes(4);
            if (!IsLittleEndian)
                Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public override uint ReadUInt32()
        {
            var data = base.ReadBytes(4);
            if (!IsLittleEndian)
                Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public string ReadFixedString(int length, Encoding encoding = null)
        {
            encoding ??= Encoding.ASCII;
            var data = base.ReadBytes(length);
            return encoding.GetString(data).TrimEnd('\0');
        }

        public System.Drawing.Color ReadRGB555()
        {
            ushort val = ReadUInt16();
            int r = (val & 0x1F) << 3;
            int g = ((val >> 5) & 0x1F) << 3;
            int b = ((val >> 10) & 0x1F) << 3;
            return System.Drawing.Color.FromArgb(r, g, b);
        }
    }
}
