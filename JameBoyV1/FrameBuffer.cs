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
    class FrameBuffer
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

        //currently loops through all the colors of the rainbow. I used it to see if i could achieve at least 60fps   

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



    }
}
