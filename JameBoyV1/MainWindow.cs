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
            //make the parts of the gameboy
            CPU cpu = CPU.Instance;
            MMU memoryManager = MMU.Instance;
            Registers reg = Registers.Instance;


            //load rom into ram
            memoryManager.initalROMLoad(File.ReadAllBytes(Properties.Settings.Default.ROMPath)); //this only works for Tetris, or Dr. Mario. most others are too big

            //inital values

            reg.AF.word = 0x01B0; //for GB or Super GB
            reg.BC.word = 0x0013;
            reg.DE.word = 0x00D8;
            reg.HL.word = 0x014D;

            reg.SP = 0xfffe;
            reg.PC = 0x0100;


            memoryManager.writeByte(0x00, 0xFF05);
            memoryManager.writeByte(0x00, 0xFF06);
            memoryManager.writeByte(0x00, 0xFF07);
            memoryManager.writeByte(0x80, 0xFF10);
            memoryManager.writeByte(0xBF, 0xFF11);
            memoryManager.writeByte(0xF3, 0xFF12);
            memoryManager.writeByte(0xBF, 0xFF14);
            memoryManager.writeByte(0x3F, 0xFF16);
            memoryManager.writeByte(0x00, 0xFF17);
            memoryManager.writeByte(0xBF, 0xFF19);
            memoryManager.writeByte(0x7F, 0xFF1A);
            memoryManager.writeByte(0xFF, 0xFF1B);
            memoryManager.writeByte(0x9F, 0xFF1C);
            memoryManager.writeByte(0xBF, 0xFF1E);
            memoryManager.writeByte(0xFF, 0xFF20);
            memoryManager.writeByte(0x00, 0xFF21);
            memoryManager.writeByte(0x00, 0xFF22);
            memoryManager.writeByte(0xBF, 0xFF23);
            memoryManager.writeByte(0x77, 0xFF24);
            memoryManager.writeByte(0xF3, 0xFF25);
            memoryManager.writeByte(0xF1, 0xFF26);
            memoryManager.writeByte(0x91, 0xFF40);
            memoryManager.writeByte(0x00, 0xFF42);
            memoryManager.writeByte(0x00, 0xFF43);
            memoryManager.writeByte(0x00, 0xFF45);
            memoryManager.writeByte(0xFC, 0xFF47);
            memoryManager.writeByte(0xFF, 0xFF48);
            memoryManager.writeByte(0xFF, 0xFF49);
            memoryManager.writeByte(0x00, 0xFF4A);
            memoryManager.writeByte(0x00, 0xFF4B);
            memoryManager.writeByte(0x00, 0xFFFF);

            //crude video
            FrameBuffer FB = new FrameBuffer();
            Graphics g = pictureBox1.CreateGraphics();
            Bitmap frame = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
            Rectangle all = new Rectangle(0, 0, frame.Width, frame.Height);
            //byte red = 0;
            //byte green = 0;
            //byte blue = 0;
            BitmapData frameData = frame.LockBits(all, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);

            IntPtr ptr = frameData.Scan0;


            int bytesNeeded = Math.Abs(frameData.Stride) * frame.Height;
            //byte[] color = new byte[bytesNeeded];


            //for (int x = 0; x<256; x++)
            //{
            //    for (int y = 0; y<256; y++)
            //    {
            //        for (int m = 0; m < bytesNeeded; m += 3)
            //        {
            //            color[m] = red;
            //            color[m + 1] = green;
            //            color[m + 2] = blue;
            //        }
            //        System.Runtime.InteropServices.Marshal.Copy(color, 0, ptr, bytesNeeded);
            //        frame.UnlockBits(frameData);

            //        g.DrawImage(frame, 0, 0);
            //        frame.LockBits(all, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);
            //        ptr = frameData.Scan0;
            //    }
            //}
            






            //Console.WriteLine(reg.PC);
            byte opcode;
            int cycleCounter = 0;
            long lastCycleCount = 0;
            do
            {
                
                opcode = memoryManager.readByte();
                //cpu process opcode
                cpu.runOpcode(opcode);
                cycleCounter += (int)(cpu.CPUCycles - lastCycleCount);
                lastCycleCount = cpu.CPUCycles;
                if(cycleCounter > 70224)//TODO:hardcoded for frame timing
                {
                    cycleCounter = 0;

                    #region draw frame
                    System.Runtime.InteropServices.Marshal.Copy(FB.fullFrame24bpp(), 0, ptr, bytesNeeded);
                    frame.UnlockBits(frameData);

                    g.DrawImage(frame, 0, 0);
                    frame.LockBits(all, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);
                    ptr = frameData.Scan0;

                    #endregion

                    //paintFrame();
                }
                
                
                //Console.Write("Opcode: {0,2:X} , A = {1,2:X}, F = {2,2:X}, B = {3,2:X}, C = {4,2:X}, D = {5,2:X}, E = {6,2:X}, H = {7,2:X}, L = {8,2:X}", opcode, reg.A, reg.F, reg.B, reg.C, reg.D, reg.E, reg.H, reg.L);
                //Console.WriteLine(cpu.CPUCycles);
                //Console.ReadLine();
                //if(cpu.CPUCycles % 0xffff == 0) { Console.WriteLine(cpu.CPUCycles); }
                //Console.ReadLine();

            } while (true);


        }

        public void paintFrame()
        {
            //Bitmap frame = new Bitmap(300, 300, PixelFormat.Format24bppRgb);
            //Rectangle all = new Rectangle(0, 0, frame.Width, frame.Height);
            //byte red = 0;
            //byte green = 0;
            //byte blue = 0;
            //BitmapData frameData = frame.LockBits(all, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);

            //IntPtr ptr = frameData.Scan0;


            //int bytesNeeded = Math.Abs(frameData.Stride) * frame.Height;
            //byte[] color = new byte[bytesNeeded];

            ////currently loops through all the colors of the rainbow.I used it to see if i could achieve at least 60fps

            //for (red = 1; red < 255; red++)
            //{
            //    for (green = 1; green < 255; green++)
            //    {
            //        for (blue = 1; blue < 255; blue++)
            //        {
            //            for (int m = 0; m < bytesNeeded; m += 3)
            //            {
            //                color[m] = red;
            //                color[m + 1] = green;
            //                color[m + 2] = blue;
            //            }
            //            System.Runtime.InteropServices.Marshal.Copy(color, 0, ptr, bytesNeeded);
            //            frame.UnlockBits(frameData);

            //            System.Graphics.(frame, 50, 50);
            //            frame.LockBits(all, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);
            //            ptr = frameData.Scan0;
            //        }
            //    }
            //}
        }

        private void pictureBox1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Create a local version of the graphics object for the PictureBox.
            Graphics g = e.Graphics;

            // Draw a string on the PictureBox.
            g.DrawString("This is a diagonal line drawn on the control",
                new Font("Arial", 10), System.Drawing.Brushes.Blue, new Point(30, 30));
            // Draw a line in the PictureBox.
            g.DrawLine(System.Drawing.Pens.Red, pictureBox1.Left, pictureBox1.Top,
                pictureBox1.Right, pictureBox1.Bottom);
        }
    }
}
