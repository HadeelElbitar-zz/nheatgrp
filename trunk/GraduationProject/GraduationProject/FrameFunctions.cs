using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace GraduationProject
{
    class FrameFunctions
    {
        public FrameFunctions() { }
        public void DisplayFrame(FrameInfo pic , PictureBox Box)
        {
            int width = pic.width;
            int height = pic.height;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                int space = bmpData.Stride - width * 3;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        p[0] = pic.bluePixels[i, j];
                        p[1] = pic.greenPixels[i, j];
                        p[2] = pic.redPixels[i, j];
                        p += 3;
                    }
                    p += space;
                }
            }
            bmp.UnlockBits(bmpData);
            pic.FrameBox = Box;
            pic.FrameBox.Size = new System.Drawing.Size(width, height);
            pic.FrameBox.Image = bmp;
        }
    }
}
