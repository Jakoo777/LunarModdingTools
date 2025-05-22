using Lunar_Modding_Tools.PckBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
* Using Rufas Wan's findings and code as a base.
* Converted from PHP to C# by Jakoo https://github.com/Jakoo777
* Found at https://lunarthreads.com/viewtopic.php?t=6189
* 
* Original credit:
* Ripped by Rufas Wan
* https://www.vg-resource.com/user-45082.html
* https://www.spriters-resource.com/submitter/rufaswan/
* https://github.com/rufaswan/Web2D_Games/tree/master/tools/psxtools
*/

namespace Lunar_Modding_Tools
{
    public class LunarPakExtractor
    {
        private readonly string _baseDir;
        private byte[] _upd, _idx, _pak;

        public static LunarPakExtractor instance { get; private set; }

        public LunarPakExtractor(string baseDir, bool forceNewInstance = false)
        {
            if (instance == null || forceNewInstance)
            {
                instance = this;
            }
            _baseDir = baseDir;
        }

        public void Extract()
        {
            LoadFiles();
            if (_upd == null || _idx == null || _pak == null)
                throw new FileNotFoundException("Required .upd, .idx, or .pak files not found.");

            int b1 = ReadInt32(_upd, 0);   // toc base
            int b2 = ReadInt32(_upd, 4);   // file count
            int b5 = ReadInt32(_upd, 16);  // filename base

            string outputDir = Path.Combine(_baseDir, "data");
            Directory.CreateDirectory(outputDir);

            StringBuilder tocBuilder = new();

            for (int i = 0; i < b2; i++)
            {
                int entryOffset = b1 + i * 0x14;

                int namePtrOffset = ReadInt32(_upd, entryOffset + 0);
                string filename = ReadNullTerminatedString(_upd, b5 + namePtrOffset).ToLower();

                int indexEntry = ReadInt32(_upd, entryOffset + 12) * 5;
                int lba = Read24BitBE(_idx, indexEntry);
                int size = Read16BitBE(_idx, indexEntry + 3);

                int realSize = ReadInt32(_upd, entryOffset + 16);  // actual byte count

                byte[] fileData = new byte[realSize];
                Array.Copy(_pak, lba * 0x800, fileData, 0, realSize);
                if (!File.Exists(Path.Combine(outputDir, filename))) 
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(outputDir, filename)));
                }

                File.WriteAllBytes(Path.Combine(outputDir, filename), fileData);

                string line = $"{lba:X6} , {lba * 0x800:X8} , {realSize:X8} , {filename}";
                tocBuilder.AppendLine(line);
            }

            File.WriteAllText(Path.Combine(outputDir, "toc.txt"), tocBuilder.ToString());
        }

        public void Replace(string fname, byte[] data)
        {
            LoadFiles();
            if (_upd == null || _idx == null || _pak == null)
                throw new FileNotFoundException("Required .upd, .idx, or .pak files not found.");

            int b1 = ReadInt32(_upd, 0);   // toc base
            int b2 = ReadInt32(_upd, 4);   // file count
            int b5 = ReadInt32(_upd, 16);  // filename base

            string outputDir = Path.Combine(_baseDir, "data");
            Directory.CreateDirectory(outputDir);

            StringBuilder tocBuilder = new();

            for (int i = 0; i < b2; i++)
            {
                int entryOffset = b1 + i * 0x14;

                int namePtrOffset = ReadInt32(_upd, entryOffset + 0);
                string filename = ReadNullTerminatedString(_upd, b5 + namePtrOffset).ToLower();

                int indexEntry = ReadInt32(_upd, entryOffset + 12) * 5;
                int lba = Read24BitBE(_idx, indexEntry);
                int size = Read16BitBE(_idx, indexEntry + 3);

                int realSize = ReadInt32(_upd, entryOffset + 16);  // actual byte count

                byte[] fileData = new byte[realSize];
                Array.Copy(_pak, lba * 0x800, fileData, 0, realSize);

                //replace here
                if (filename.Contains(fname)) 
                {
                    Array.Copy(data, 0, _pak, lba * 0x800, realSize);
                }
                

                string line = $"{lba:X6} , {lba * 0x800:X8} , {realSize:X8} , {filename}";
                tocBuilder.AppendLine(line);
            }

            File.WriteAllBytes(_baseDir + "\\DATA.PAK", _pak);
        }

        private void LoadFiles()
        {
            _upd = TryLoad(Path.Combine(_baseDir, "data.upd"));
            _idx = TryLoad(Path.Combine(_baseDir, "data.idx"));
            _pak = TryLoad(Path.Combine(_baseDir, "data.pak"));
        }

        private byte[] TryLoad(string path) => File.Exists(path) ? File.ReadAllBytes(path) : null;

        private int ReadInt32(byte[] buf, int offset)
            => BitConverter.ToInt32(buf, offset);

        private int Read24BitBE(byte[] buf, int offset)
            => (buf[offset] << 16) | (buf[offset + 1] << 8) | buf[offset + 2];

        private int Read16BitBE(byte[] buf, int offset)
            => (buf[offset] << 8) | buf[offset + 1];

        private string ReadNullTerminatedString(byte[] buf, int offset)
        {
            int end = offset;
            while (end < buf.Length && buf[end] != 0) end++;
            return Encoding.ASCII.GetString(buf, offset, end - offset);
        }

        public static void LoadPck(ref List<byte[]> fileList, byte[] file, string fname)
        {
            int num = BitConverter.ToInt32(file, 0);
            Console.WriteLine($"== LoadPck( {fname} ) = {num}");

            int ed = file.Length;
            int st = 4;
            fileList = new List<byte[]>();

            while (st < ed)
            {
                int siz = BitConverter.ToInt32(file, st);
                st += 4;
                if (siz == 0)
                    break;

                Console.WriteLine($"{fileList.Count,3}  {st:X6}  {siz:X6}");
                byte[] chunk = new byte[siz];
                Buffer.BlockCopy(file, st, chunk, 0, siz);
                fileList.Add(chunk);
                st += siz;
            }
        }

        public static byte[] PckToClut(string fname, byte[] o = null, int index = 4)
        {
            if (!fname.Contains(".pck", StringComparison.OrdinalIgnoreCase))
                return new byte[0];

            if (fname.Contains("efspr", StringComparison.OrdinalIgnoreCase))
            {
                var fn2 = fname.Replace("efspr", "efcha", StringComparison.OrdinalIgnoreCase);
                return new byte[0];
            }
            if (fname.Contains("efcha", StringComparison.OrdinalIgnoreCase))
            {
                var fn2 = fname.Replace("efcha", "efspr", StringComparison.OrdinalIgnoreCase);
                return new byte[0];
            }

            if (fname.Contains("mnspr", StringComparison.OrdinalIgnoreCase))
            {
                var fn2 = fname.Replace("mnspr", "mncha", StringComparison.OrdinalIgnoreCase);
                return new byte[0];
            }
            if (fname.Contains("mncha", StringComparison.OrdinalIgnoreCase))
            {
                var fn2 = fname.Replace("mncha", "mnspr", StringComparison.OrdinalIgnoreCase);
                return new byte[0];
            }

            if (fname.Contains("sysspr", StringComparison.OrdinalIgnoreCase))
            {
                var fn2 = fname.Replace("sysspr", "syscha", StringComparison.OrdinalIgnoreCase);
                return new byte[0];
            }
            if (fname.Contains("syscha", StringComparison.OrdinalIgnoreCase))
            {
                var fn2 = fname.Replace("syscha", "sysspr", StringComparison.OrdinalIgnoreCase);
                return new byte[0];
            }

            if (fname.Contains("btlbk", StringComparison.OrdinalIgnoreCase))
            {
                if (o == null)
                {
                    return btlbkHandler.PckBtlbk(File.ReadAllBytes(fname), fname);
                }
                else 
                {
                    return btlbkHandler.PckBtlbk(o, fname);
                }
                
            }

            if (fname.Contains("_pc", StringComparison.OrdinalIgnoreCase))
            {
                if (o == null)
                {
                    return pcHandler.PckPc(File.ReadAllBytes(fname), fname, index);
                }
                else
                {
                    return pcHandler.PckPc(o, fname);
                }
            }

            if (fname.Contains("continue", StringComparison.OrdinalIgnoreCase))
            {
                return new byte[0];
            }

            if (fname.Contains("title", StringComparison.OrdinalIgnoreCase))
            {
                return new byte[0];
            }

            return new byte[0];
        }



    }
}
