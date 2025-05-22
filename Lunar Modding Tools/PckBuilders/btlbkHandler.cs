using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lunar_Modding_Tools.PckBuilders
{
    public static class btlbkHandler
    {
        public static byte[] PckBtlbk(byte[] file, string fname)
        {
            Console.WriteLine($"== PckBtlbk({fname})");

            if (file == null || file.Length == 0)
                return null;

            List<byte[]> pck = Helper.LoadPck(ref file, fname);
            if (pck.Count < 2)
                return null;

            int w = pck[0].Length / 0x100;
            int h = 0x100;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Header "CLUT"
                writer.Write(Encoding.ASCII.GetBytes("CLUT"));
                writer.Write(BitConverter.GetBytes(0x100)); // number of colors
                writer.Write(BitConverter.GetBytes(w));     // width
                writer.Write(BitConverter.GetBytes(h));     // height

                // Palette data (converted from RGB555 to RGBA8888)
                writer.Write(Helper.Convert555To8888(pck[1]));

                // Pixel data (raw)
                writer.Write(pck[0]);

                return ms.ToArray();
            }
        }

        public static byte[] RePckBtlbk(byte[] clut) 
        {
            DataWriter writer = new DataWriter();
            writer.Write(2); // one for clut, one for image

            DataReader reader = new DataReader(clut);
            reader.ReadBytes(4); // skip "CLUT"
            int numColors = reader.ReadInt32();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            byte[] clutData = reader.ReadBytes(numColors * 4);
            byte[] imageData = reader.ReadBytes(width * height);

            // write all the color data to 555
            DataWriter pck1 = new DataWriter();
            DataReader clutReader = new DataReader(clutData);
            pck1.Write(clutData.Length / 2); //rgb555 is half the size of rgba8888
            for (int i = 0; i < numColors; i++) 
            {
                pck1.Write(Helper.Convert8888To555(clutReader.ReadBytes(4)));
            }

            writer.Write(imageData.Length);
            writer.Write(imageData);
            writer.Write(pck1.ToArray().Length);
            writer.Write(pck1.ToArray());

            return writer.ToArray();
        }
    }
}
