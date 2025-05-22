using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lunar_Modding_Tools
{
    internal class Helper
    {
        public static List<byte[]> LoadPck(ref byte[] file, string fname)
        {
            int num = BitConverter.ToInt32(file, 0);
            Console.WriteLine($"== loadpck({fname}) = {num}");

            int ed = file.Length;
            int st = 4;
            var pck = new List<byte[]>();

            while (st < ed)
            {
                int siz = BitConverter.ToInt32(file, st);
                st += 4;

                if (siz == 0)
                    break;

                Console.WriteLine($"{pck.Count,3}  {st:X6}  {siz:X6}");

                byte[] chunk = new byte[siz];
                Array.Copy(file, st, chunk, 0, siz);
                pck.Add(chunk);
                st += siz;
            }

            return pck;
        }

        public static byte[] SavePck(List<byte[]?> files, string fname)
        {
            DataWriter writer = new DataWriter();
            writer.Write(files.Count);
            foreach (var file in files)
            {
                if (file == null)
                    continue;
                writer.Write(file.Length);
                writer.Write(file);
            }
            return writer.ToArray();

        }


        public static byte[] Convert555To8888(byte[] str)
        {
            int count = str.Length / 2;
            byte[] clut = new byte[count * 4];
            DataReader reader = new DataReader(str, true);

            for (int i = 0; i < count; i++)
            {
                // Read 2 bytes (big-endian)

                Color c = reader.ReadRGB555();
                int hi = str[i * 2];
                int lo = str[i * 2 + 1];
                int pal = (hi << 8) | lo;



                int b = c.B;
                int g = c.G;
                int r = c.R;

                // Alpha is always 255 (opaque)
                clut[i * 4 + 0] = (byte)r;
                clut[i * 4 + 1] = (byte)g;
                clut[i * 4 + 2] = (byte)b;
                clut[i * 4 + 3] = 255;

                int dbg = 1;
                if (i == 235)
                    dbg = 1;

                if (c.R == 24 && c.G == 8 && c.B == 0)
                    dbg = 1;
            }

            return clut;
        }

        public static byte[] Convert8888To555(byte[] str)
        {
            int count = str.Length / 4;
            byte[] clut = new byte[count * 2];
            DataWriter writer = new DataWriter();
            for (int i = 0; i < count; i++)
            {
                // Read 4 bytes (RGBA)
                byte r = str[i * 4 + 0];
                byte g = str[i * 4 + 1];
                byte b = str[i * 4 + 2];
                byte a = str[i * 4 + 3];
                // Convert to RGB555
                ushort rgb555 = (ushort)((r >> 3) << 10 | (g >> 3) << 5 | (b >> 3));
                clut[i * 2] = (byte)(rgb555 >> 8);
                clut[i * 2 + 1] = (byte)(rgb555 & 0xFF);
            }
            return clut;
        }

        public static byte[] BitmapToBytes(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Bmp);
                return ms.ToArray();
            }
        }

        public static Bitmap BytesToBitmap(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return new Bitmap(ms);
            }
        }
    }
}
