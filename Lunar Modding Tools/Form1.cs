using Lunar_Modding_Tools.PckBuilders;
using System.Text;

namespace Lunar_Modding_Tools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "PAL files (*.pal)|*.pal|All files (*.*)|*.*";
                ofd.Title = "Open a PAL file";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string filePath = ofd.FileName;

                    var colors = new List<Color>();
                    using (var fs = File.OpenRead(filePath))
                    using (var reader = new DataReader(fs))
                    {
                        reader.IsLittleEndian = false;
                        while (fs.Position < fs.Length)
                        {
                            colors.Add(reader.ReadRGB555());
                        }
                    }

                    ShowPaletteWindow(colors);
                }
            }
        }

        private void ShowPaletteWindow(List<Color> colors)
        {
            Form paletteForm = new Form
            {
                Text = "Palette Viewer",
                Width = 1280,
                Height = 720,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            PictureBox pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = RenderPaletteImage(colors),
                SizeMode = PictureBoxSizeMode.Normal
            };

            paletteForm.Controls.Add(pictureBox);
            paletteForm.Show();
        }

        private Bitmap RenderPaletteImage(List<Color> colors)
        {
            int cellSize = 8;
            int cols = 32;
            int rows = (int)Math.Ceiling(colors.Count / (double)cols);
            Bitmap bmp = new Bitmap(cols * cellSize, rows * cellSize);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
                for (int i = 0; i < colors.Count; i++)
                {
                    int x = (i % cols) * cellSize;
                    int y = (i / cols) * cellSize;
                    using (Brush b = new SolidBrush(colors[i]))
                    {
                        g.FillRectangle(b, x, y, cellSize, cellSize);
                    }
                    g.DrawRectangle(Pens.Gray, x, y, cellSize, cellSize);
                }
            }

            return bmp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Step 1: Load .pal file
            List<Color> palette = new List<Color>();
            using (OpenFileDialog palDialog = new OpenFileDialog())
            {
                palDialog.Filter = "PAL files (*.pal)|*.pal|All files (*.*)|*.*";
                palDialog.Title = "Select a .pal file";

                if (palDialog.ShowDialog() != DialogResult.OK)
                    return;

                using (var fs = File.OpenRead(palDialog.FileName))
                using (var reader = new DataReader(fs))
                {
                    reader.IsLittleEndian = false;
                    while (fs.Position < fs.Length)
                    {
                        palette.Add(reader.ReadRGB555());
                    }
                }
            }

            // Step 2: Load .dat file (4-bit indexes packed into bytes)
            byte[] data;
            using (OpenFileDialog datDialog = new OpenFileDialog())
            {
                datDialog.Filter = "DAT files (*.dat)|*.dat|All files (*.*)|*.*";
                datDialog.Title = "Select a .dat file";

                if (datDialog.ShowDialog() != DialogResult.OK)
                    return;

                data = File.ReadAllBytes(datDialog.FileName);
            }

            // Step 3: Convert to 4-bit palette index array (2 pixels per byte)
            List<byte> indices = new List<byte>(data.Length * 2);
            foreach (byte b in data)
            {
                byte low = (byte)(b & 0x0F);       // lower 4 bits
                byte high = (byte)((b >> 4) & 0x0F); // upper 4 bits
                indices.Add(high);
                indices.Add(low);
            }

            // Step 4: Make image square-ish
            int pixelCount = indices.Count;
            int size = (int)Math.Ceiling(Math.Sqrt(pixelCount));
            Bitmap image = new Bitmap(size, size);

            for (int i = 0; i < pixelCount; i++)
            {
                int x = i % size;
                int y = i / size;
                byte index = indices[i];

                Color color = index < palette.Count ? palette[index] : Color.Magenta;
                image.SetPixel(x, y, color);
            }

            // Step 5: Show the image
            Form imageForm = new Form
            {
                Text = "4-bit Indexed Image Viewer",
                Width = image.Width + 20,
                Height = image.Height + 40
            };

            PictureBox pictureBox = new PictureBox
            {
                Dock = DockStyle.Top,
                Image = image,
                SizeMode = PictureBoxSizeMode.Normal
            };

            imageForm.Controls.Add(pictureBox);
            imageForm.Show();
        }

        private void ExtractLunar2()
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select any file inside the game data folder";
            ofd.Filter = "Any data file|*.upd;*.idx;*.pak|All files|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string folder = Path.GetDirectoryName(ofd.FileName);
                LunarPakExtractor extractor = new LunarPakExtractor(folder, true);
                extractor.Extract();
                MessageBox.Show("Extraction complete!", "Success");
                try
                {

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to extract: " + ex.Message);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ExtractLunar2();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Lunar .pck files (*.pck)|*.pck|All files (*.*)|*.*";
                ofd.Title = "Open Lunar PCK File";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    byte[] clut = LunarPakExtractor.PckToClut(ofd.FileName, null);
                    Bitmap image = LunarPckExtractor.ImgToBmp(clut);
                    File.WriteAllBytes(ofd.FileName + ".bmp", Helper.BitmapToBytes(image));
                    //vertical flip image
                    //if (image != null)
                    //{
                    //    for (int y = 0; y < image.Height / 2; y++)
                    //    {
                    //        for (int x = 0; x < image.Width; x++)
                    //        {
                    //            Color temp = image.GetPixel(x, y);
                    //            image.SetPixel(x, y, image.GetPixel(x, image.Height - 1 - y));
                    //            image.SetPixel(x, image.Height - 1 - y, temp);
                    //        }
                    //    }
                    //}
                    try
                    {


                        // Show image in new window
                        Form preview = new Form
                        {
                            Text = Path.GetFileName(ofd.FileName),
                            ClientSize = new Size(image.Width, image.Height)
                        };

                        PictureBox pb = new PictureBox
                        {
                            Image = image,
                            Dock = DockStyle.Fill,
                            SizeMode = PictureBoxSizeMode.Zoom
                        };

                        preview.Controls.Add(pb);
                        preview.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            byte[] bmp = File.ReadAllBytes("C:\\Program Files (x86)\\Steam\\steamapps\\common\\LUNAR Remastered Collection\\data2_new\\DATA_jp\\ISOK\\_PC001.PCK.bmp");
            Bitmap bitmap = new Bitmap(new MemoryStream(bmp));
            byte[] clut = LunarPckExtractor.BitmapToClut(bitmap, true);

            byte[] pck = pcHandler.RePckPc(clut);
            File.WriteAllBytes("C:\\Program Files (x86)\\Steam\\steamapps\\common\\LUNAR Remastered Collection\\data2_new\\DATA_jp\\ISOK\\_pc001.pck.new", pck);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            //prompt to select a file
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Lunar .pck files (*.pck)|*.pck|All files (*.*)|*.*";
                ofd.Title = "Open Lunar PCK File";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using OpenFileDialog ofd2 = new OpenFileDialog();
                    ofd2.Title = "Select any file inside the game data folder";
                    ofd2.Filter = "Any data file|*.upd;*.idx;*.pak|All files|*.*";

                    if (ofd2.ShowDialog() == DialogResult.OK)
                    {
                        string folder = Path.GetDirectoryName(ofd2.FileName);
                        LunarPakExtractor extractor = new LunarPakExtractor(folder, true);
                        extractor.Replace(Path.GetFileName(ofd.FileName), File.ReadAllBytes(ofd.FileName));
                        MessageBox.Show("Replace complete!", "Success");
                        try
                        {

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to extract: " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}
