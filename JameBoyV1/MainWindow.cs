using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JameBoyV1
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }

        private void openAndRunToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void loadROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = Properties.Settings.Default.DefaultROMDir;
            openFileDialog1.Filter = "GameBoy ROM Files (*.gb)|*.gb|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        GlobalData.cartridgeROM = File.ReadAllBytes(openFileDialog1.FileName);
                        Properties.Settings.Default.ROMPath = openFileDialog1.FileName;
                        textBoxMessages.Text = "Filename of ROM loaded: " + Properties.Settings.Default.ROMPath;

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void buttonStartEmulation_Click(object sender, EventArgs e)
        {

            Bitmap frame = new Bitmap(300, 300, PixelFormat.Format24bppRgb);
            Rectangle all = new Rectangle(0, 0, frame.Width, frame.Height);
            byte red = 0;
            byte green = 0;
            byte blue = 0;
            BitmapData frameData = frame.LockBits(all, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);

            IntPtr ptr = frameData.Scan0;


            int bytesNeeded = Math.Abs(frameData.Stride) * frame.Height;
            byte[] color = new byte[bytesNeeded];

            //currently loops through all the colors of the rainbow.I used it to see if i could achieve at least 60fps

                for (red = 1; red < 255; red++)
            {
                for (green = 1; green < 255; green++)
                {
                    for (blue = 1; blue < 255; blue++)
                    {
                        for (int m = 0; m < bytesNeeded; m += 3)
                        {
                            color[m] = red;
                            color[m + 1] = green;
                            color[m + 2] = blue;
                        }
                        System.Runtime.InteropServices.Marshal.Copy(color, 0, ptr, bytesNeeded);
                        frame.UnlockBits(frameData);

                        System.Graphics.(frame, 50, 50);
                        frame.LockBits(all, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);
                        ptr = frameData.Scan0;
                    }
                }
            }
        }

        private void pictureBox1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Create a local version of the graphics object for the PictureBox.
            Graphics g = e.Graphics;

            g.DrawImage(sender);
            // Draw a string on the PictureBox.
            g.DrawString("This is a diagonal line drawn on the control",
                new Font("Arial", 10), System.Drawing.Brushes.Blue, new Point(30, 30));
            // Draw a line in the PictureBox.
            g.DrawLine(System.Drawing.Pens.Red, pictureBox1.Left, pictureBox1.Top,
                pictureBox1.Right, pictureBox1.Bottom);
        }
    }
}
