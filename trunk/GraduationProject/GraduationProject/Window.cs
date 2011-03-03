using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using openCV;
using Emgu.CV.UI;

namespace GraduationProject
{
    class Window
    {
        public int Center_X, Center_Y, width, height;
        public byte[,] Rpixel, Gpixel, Bpixel;
        public IplImage Lab;
        Frame TempWinFrame;
        static int Counter = 0;
        public Window(int _width , int _height, Frame _Frame, CvPoint _CenterPoint) 
        {
            height = _height;
            width = _width;
            if (_width % 2 == 0) width++;
            if (_height % 2 == 0) height++;

            Center_X = _CenterPoint.x;
            Center_Y = _CenterPoint.y;

            Rpixel = new byte[height, width];
            Gpixel = new byte[height, width];
            Bpixel = new byte[height, width];

            Lab = new IplImage();
            Lab.width = width;
            Lab.height = height;
            Lab.nChannels = 3;

            int M = (width - 1) / 2, N = (height - 1) / 2;
            for (int i = Center_Y - N , c = 0; i < _Frame.height && i < (Center_Y + N) && c < height; i++, c++)
            {
                if (i < 0) i = 0;
                for (int j = Center_X - M, k = 0; j < _Frame.width && j < (Center_X + M) && k < width; j++, k++)
                {
                    if (j < 0) j = 0;
                    Rpixel[c, k] = _Frame.redPixels[i, j];
                    Gpixel[c, k] = _Frame.greenPixels[i, j];
                    Bpixel[c, k] = _Frame.bluePixels[i, j];
                }
            }
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

            string Nw = "FrameTest" + Counter.ToString() + ".bmp";
            Counter++;
            string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + Nw;
            
            TempWinFrame = new Frame(width, height, null, Rpixel, Gpixel, Bpixel, Lab, Pw);
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                int space = bmpData.Stride - _Frame.width * 3;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        p[0] = Bpixel[i, j];
                        p[1] = Gpixel[i, j];
                        p[2] = Rpixel[i, j];
                        p += 3;
                    }
                    p += space;
                }
            }
            bmp.UnlockBits(bmpData);
            bmp.Save(Pw, ImageFormat.Bmp);
            TempWinFrame.RGB2Lab(Pw);
        }
    }
}
