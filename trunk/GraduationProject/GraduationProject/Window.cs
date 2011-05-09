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
        public int Center_X, Center_Y;
        public Frame WinFrame, BinaryWinFrame;
        public Classifier WinClassifier;
        static int Counter = 0;
        public int[,] AfterCalcPointClass;
        public Window(int _width, int _height, Frame _FrameMask, Frame _Frame, CvPoint _CenterPoint) 
        {
            WinFrame = new Frame();
            WinFrame.height = _height;
            WinFrame.width = _width;
            if (_width % 2 == 0) WinFrame.width++;
            if (_height % 2 == 0) WinFrame.height++;

            BinaryWinFrame = new Frame();
            BinaryWinFrame.height = _height;
            BinaryWinFrame.width = _width;
            if (_width % 2 == 0) BinaryWinFrame.width++;
            if (_height % 2 == 0) BinaryWinFrame.height++;

            Center_X = _CenterPoint.x;
            Center_Y = _CenterPoint.y;

            WinFrame.redPixels = new byte[WinFrame.height, WinFrame.width];
            WinFrame.greenPixels = new byte[WinFrame.height, WinFrame.width];
            WinFrame.bluePixels = new byte[WinFrame.height, WinFrame.width];

            BinaryWinFrame.redPixels = new byte[BinaryWinFrame.height, BinaryWinFrame.width];
            BinaryWinFrame.greenPixels = new byte[BinaryWinFrame.height, BinaryWinFrame.width];
            BinaryWinFrame.bluePixels = new byte[BinaryWinFrame.height, BinaryWinFrame.width];

            AfterCalcPointClass = new int[WinFrame.height, WinFrame.width];
           // WinFrame.Lab = 

            int M = (WinFrame.width - 1) / 2, N = (WinFrame.height - 1) / 2;
            for (int i = Center_Y - N , c = 0; i < _Frame.height && i < (Center_Y + N) && c < WinFrame.height; i++, c++)
            {
                if (i < 0) i = 0;
                for (int j = Center_X - M, k = 0; j < _Frame.width && j < (Center_X + M) && k < WinFrame.width; j++, k++)
                {
                    if (j < 0) j = 0;
                    WinFrame.redPixels[c, k] = _Frame.redPixels[i, j];
                    WinFrame.greenPixels[c, k] = _Frame.greenPixels[i, j];
                    WinFrame.bluePixels[c, k] = _Frame.bluePixels[i, j];

                    BinaryWinFrame.redPixels[c, k] = _FrameMask.redPixels[i, j];
                    BinaryWinFrame.greenPixels[c, k] = _FrameMask.greenPixels[i, j];
                    BinaryWinFrame.bluePixels[c, k] = _FrameMask.bluePixels[i, j];
                }
            }
            WinFrame.BmpImage = new Bitmap(WinFrame.width, WinFrame.height);
            BitmapData bmpData = WinFrame.BmpImage.LockBits(new Rectangle(0, 0, WinFrame.width, WinFrame.height), System.Drawing.Imaging.ImageLockMode.ReadWrite, WinFrame.BmpImage.PixelFormat);
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                int space = bmpData.Stride - WinFrame.width * 3;
                for (int i = 0; i < WinFrame.height; i++)
                {
                    for (int j = 0; j < WinFrame.width; j++)
                    {
                        p[0] = WinFrame.bluePixels[i, j];
                        p[1] = WinFrame.greenPixels[i, j];
                        p[2] = WinFrame.redPixels[i, j];
                        p += 3;
                    }
                    p += space;
                }
            }
            WinFrame.BmpImage.UnlockBits(bmpData);

            WinFrame.RgbImage = new Image<Bgr, byte>(WinFrame.BmpImage);
            WinFrame.LabImage = WinFrame.RgbImage.Convert<Lab, byte>();
            WinFrame.RGB = (IplImage)cvtools.ConvertPtrToStructure(WinFrame.RgbImage.Ptr, typeof(IplImage));
            WinFrame.Lab = (IplImage)cvtools.ConvertPtrToStructure(WinFrame.LabImage.Ptr, typeof(IplImage));

            string Nw = "FrameTest" + Counter.ToString() + ".bmp";
            Counter++;
            string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + Nw;

            WinFrame.BmpImage.Save(Pw, ImageFormat.Bmp);
            WinClassifier = new Classifier();
            WinClassifier.TrainClassifier(this);
        }
    }
}
