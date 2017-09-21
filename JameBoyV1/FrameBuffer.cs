using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;


namespace JameBoyV1
{
    public class FrameBuffer
    {

        //Code to draw a frame. needs to be run in ui thread.
        //Graphics g = pe.Graphics;
        //Bitmap frame = new Bitmap(300, 300, PixelFormat.Format24bppRgb);
        //Rectangle all = new Rectangle(0, 0, frame.Width, frame.Height);
        //byte red = 0;
        //byte green = 0;
        //byte blue = 0;
        //BitmapData frameData = frame.LockBits(all, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);

        //IntPtr ptr = frameData.Scan0;


        //int bytesNeeded = Math.Abs(frameData.Stride) * frame.Height;
        //byte[] color = new byte[bytesNeeded];

        ////currently loops through all the colors of the rainbow. I used it to see if i could achieve at least 60fps   

        //    for (red = 1; red< 255; red++)
        //    {
        //        for (green = 1; green< 255; green++) 
        //        {
        //            for (blue = 1; blue< 255; blue++)
        //            {
        //                for (int m = 0; m<bytesNeeded; m+=3)
        //                {
        //                    color[m] = red;
        //                    color[m + 1] = green;
        //                    color[m + 2] = blue;
        //                }
        //                System.Runtime.InteropServices.Marshal.Copy(color, 0, ptr, bytesNeeded);
        //                frame.UnlockBits(frameData);

        //                g.DrawImage(frame, 50, 50);
        //                frame.LockBits(all, System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);
        //                ptr = frameData.Scan0;
        //            }
        //        }
        //    }

        //public byte LCDControlRegister;
        private MMU vram = MMU.Instance;


        public int[] objectPalette0Data
        {
            get
            {
                return vram.OBPalette0;
            }
        }
        public int[] objectPalette1Data
        {
            get
            {
                return vram.OBPalette1;
            }
        }

        public byte DMATransferStartAddress;

        public FrameBuffer()
        {

        }

        

        public byte[] fullFrame24bpp()
        {
            byte[] framePixels = new byte[3*256*256];
            int[,] frameData = fullFrame();
            int[,] frameColoured = new int[256, 256];

            for(int y=0;y<256;y++)
            {
                for(int x=0;x<256;x++)
                {
                    framePixels[(x + y*256) * 3] = (byte)(frameData[x, y] * 85);
                    framePixels[(x + y * 256) * 3] = (byte)(frameData[x, y] * 85);
                    framePixels[(x + y * 256) * 3] = (byte)(frameData[x, y] * 85);
                }
            }

            return framePixels;
                
        }
        public int[,] fullFrame()
        {
            
            int[,] frameData = new int[256,256];
            int[] palette = vram.BGPalette;
            byte[,] BGTileMap = vram.BGTileMap();
            Dictionary<int, Tile> tileSet = vram.TileSet();
            Tile[,] tileMap = new Tile[32, 32];


            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    tileMap[i, j] = tileSet[BGTileMap[i, j]];
                }
            }

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    frameData[i, j] = palette[tileMap[i / 32, j / 32].pixelMap[i%32, j%32]];
                }
            }

            return frameData;
        }

    }

    public class Tile
    {
        public Tile(List<byte> tileData)
        {
            populatePixelMap(tileData);
        }

        //4 different colours, little endian, 8x8, 16 bytes total
        //starts at top left corner
        public int[,] pixelMap;


        private void populatePixelMap(List<byte> tileData)
        {
            int pixel;
            pixelMap = new int[160, 144];
            //check size.
            if (tileData.Count() == 16)
            {

                for (int i =0; i<8;i++)
                {
                    byte firstByte = tileData[2 * i];
                    byte secondByte = tileData[2 * i + 1];

                    for (int j=0; j<8; j++)
                    {
                        pixel = 0;

                        pixel = pixel & ((firstByte & (1 << (7 - j))) > 0 ? 1 : 0);
                        pixel = pixel & ((secondByte & (1 << (7 - j))) > 0 ? 2 : 0);

                        pixelMap[i, j] = pixel;
                    }
                }  
            }
            else
            { //do nothing
            }
        }
    }
}
