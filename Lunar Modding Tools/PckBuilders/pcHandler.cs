using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Lunar_Modding_Tools.PckBuilders
{
    public class pcHandler
    {
        public static byte[] palette;
        public static byte[] tst1;
        public static byte[] tst2;
        public static byte[] tst3;

        public static byte[] GetSysDataPalette(byte[] file, int index = 4) 
        {
            if (file == null || file.Length == 0)
                return null;

            List<byte[]> pck = Helper.LoadPck(ref file, "");
            palette = pck[4];
            return pck[4]; //palette
        }

        public static byte[] PckPc(byte[] file, string fname, int index = 4)
        {
            byte paletteShift = 0;
            if (file == null || file.Length == 0)
                return null;

            List<byte[]> pck = Helper.LoadPck(ref file, fname);
            //for now
            byte[] sysdata = System.IO.File.ReadAllBytes("C:\\Program Files (x86)\\Steam\\steamapps\\common\\LUNAR Remastered Collection\\data2_new\\DATA_jp\\ISOK\\sysspr.pck");
            byte[] pal = GetSysDataPalette(sysdata, index);

            int w = 0;
            if (pck.Count == 3)
            {
                w = pck[2].Length / 0x100;

                tst1 = pck[0];
                tst2 = pck[1];
                tst3 = pck[2];
            }
            else 
            {
                w = pck[1].Length / 0x100;
            }
            
            int h = 0x100;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Header "CLUT"
                writer.Write(Encoding.ASCII.GetBytes("CLUT"));
                writer.Write(BitConverter.GetBytes(pal.Length / 2)); // number of colors
                writer.Write(BitConverter.GetBytes(w));     // width
                writer.Write(BitConverter.GetBytes(h));     // height

                // Palette data (converted from RGB555 to RGBA8888)
                writer.Write(Helper.Convert555To8888(pal));

                // Pixel data (raw)

                if (pck.Count == 3)
                {
                    for (int i = 0; i < pck[2].Length; i++) 
                    {
                        pck[2][i] += paletteShift;
                    }
                    writer.Write(pck[2]);
                    
                }
                else 
                {
                    writer.Write(pck[1]);
                }
                    

                return ms.ToArray();
            }
        }

        public static byte[] RePckPc(byte[] clut)
        {
            DataWriter writer = new DataWriter();
            writer.Write(3); // forced 3 for now

            DataReader reader = new DataReader(clut);
            reader.ReadBytes(4); // skip "CLUT"
            int numColors = reader.ReadInt32();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            reader.ReadBytes(numColors * 4); //skip colors
            byte[] imageData = reader.ReadBytes(width * height);

            writer.Write(tst1.Length);
            writer.Write(tst1);
            writer.Write(tst2.Length);
            writer.Write(tst2);
            writer.Write(imageData.Length);
            writer.Write(imageData);

            return writer.ToArray();
        }
    }
}
