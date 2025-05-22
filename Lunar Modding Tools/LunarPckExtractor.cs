using Lunar_Modding_Tools;
using Lunar_Modding_Tools.PckBuilders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public class LunarPckExtractor
{
    public static Bitmap ImgToBmp(byte[] clut)
    {
        if (clut.Length == 0)
            return null;

        string mgc = System.Text.Encoding.ASCII.GetString(clut, 0, 4);
        if (mgc == "CLUT")
            return ClutToBmp(clut);
        if (mgc == "RGBA")
            return RgbaToBmp(clut);

        return null;
    }

    private static Bitmap ClutToBmp(byte[] clut)
    {
        int cc = BitConverter.ToInt32(clut, 4);
        int w = BitConverter.ToInt32(clut, 8);
        int h = BitConverter.ToInt32(clut, 12);

        List<Color> palette = new List<Color>();
        int pos = 0x10;
        for (int i = 0; i < cc; i++)
        {
            byte r = clut[pos];
            byte g = clut[pos + 1];
            byte b = clut[pos + 2];
            byte a = clut[pos + 3];
            palette.Add(Color.FromArgb(a, r, g, b));
            pos += 4;
        }

        Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        for (int y = 0; y < h; y++)
        {
            int rowPos = 0x10 + (cc * 4) + ((h - 1 - y) * w);
            for (int x = 0; x < w; x++)
            {
                byte px = clut[rowPos + x];
                bmp.SetPixel(x, y, palette[px]);
            }
        }

        return bmp;
    }

    private static Bitmap RgbaToBmp(byte[] rgba)
    {
        int w = BitConverter.ToInt32(rgba, 4);
        int h = BitConverter.ToInt32(rgba, 8);

        Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        for (int y = 0; y < h; y++)
        {
            int rowPos = 12 + ((h - 1 - y) * w * 4);
            for (int x = 0; x < w; x++)
            {
                byte r = rgba[rowPos + x * 4];
                byte g = rgba[rowPos + x * 4 + 1];
                byte b = rgba[rowPos + x * 4 + 2];
                byte a = rgba[rowPos + x * 4 + 3];
                bmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));
            }
        }

        return bmp;
    }

    public static byte[] BitmapToClut(Bitmap bmp, bool usePCPalette = false)
    {
        if (bmp == null)
            return null;

        int w = bmp.Width;
        int h = bmp.Height;
        int cc = 0;
        List<Color> uniqueColors = new List<Color>();
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color pixelColor = bmp.GetPixel(x, y);
                if (!uniqueColors.Contains(pixelColor))
                {
                    uniqueColors.Add(pixelColor);
                    cc++;
                }
            }
        }
        if (usePCPalette)
            cc = Math.Min(pcHandler.palette.Length / 2, 256); // Assuming a maximum of 256 colors for the palette
        else
            cc = Math.Min(cc, 256); // Assuming a maximum of 256 colors for the palette


        byte[] clut = new byte[0x10 + (cc * 4) + (w * h)];
        Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("CLUT"), 0, clut, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cc), 0, clut, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(w), 0, clut, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(h), 0, clut, 12, 4);
        int pos = 0x10;
        if (usePCPalette)
        {
            uniqueColors = new List<Color>();
            DataReader reader = new DataReader(pcHandler.palette, true);
            for (int i = 0; i < pcHandler.palette.Length / 2; i++)
            {
                Color pixelColor = reader.ReadRGB555();
                if(pixelColor.R == 24 && pixelColor.G == 8 && pixelColor.B == 0)
                    if (!uniqueColors.Contains(pixelColor))
                    {

                    }
                uniqueColors.Add(pixelColor);
                if (!uniqueColors.Contains(pixelColor))
                {
                    
                }
            }

        }
        foreach (Color color in uniqueColors)
        {
            clut[pos++] = color.R;
            clut[pos++] = color.G;
            clut[pos++] = color.B;
            clut[pos++] = color.A;
        }

        pos = 0x10 + (cc * 4);
        
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color pixelColor = bmp.GetPixel(x, y);
                int index = Array.IndexOf(uniqueColors.ToArray(), pixelColor);
                clut[pos++] = (byte)index;
            }
        }
        return clut;
    }


}
