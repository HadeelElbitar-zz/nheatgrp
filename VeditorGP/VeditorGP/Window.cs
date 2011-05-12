using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using openCV;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;

namespace VeditorGP
{
    class Window
    {
        public int Center_X, Center_Y, WindowSize;
        public Frame WindowFrame, WindowBinaryMask;
        public Classifier WindowClassifier;
        static int Counter = 0;
        public Window(int width, int height, Frame ColoredFrame, Frame BinaryMask, Point CenterPoint)
        {
            WindowFrame = new Frame();
            WindowFrame.height = height;
            WindowFrame.width = width;
            if (width % 2 == 0) WindowFrame.width++;
            if (height % 2 == 0) WindowFrame.height++;

            WindowBinaryMask = new Frame();
            WindowBinaryMask.height = height;
            WindowBinaryMask.width = width;
            if (width % 2 == 0) WindowBinaryMask.width++;
            if (height % 2 == 0) WindowBinaryMask.height++;

            WindowSize = WindowFrame.width * WindowFrame.height;
            Center_X = CenterPoint.X;
            Center_Y = CenterPoint.Y;

            WindowFrame.byteRedPixels = new byte[WindowFrame.height, WindowFrame.width];
            WindowFrame.byteGreenPixels = new byte[WindowFrame.height, WindowFrame.width];
            WindowFrame.byteBluePixels = new byte[WindowFrame.height, WindowFrame.width];

            WindowBinaryMask.byteRedPixels = new byte[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.byteGreenPixels = new byte[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.byteBluePixels = new byte[WindowBinaryMask.height, WindowBinaryMask.width];

            WindowFrame.doubleRedPixels = new double[WindowFrame.height, WindowFrame.width];
            WindowFrame.doubleGreenPixels = new double[WindowFrame.height, WindowFrame.width];
            WindowFrame.doubleBluePixels = new double[WindowFrame.height, WindowFrame.width];

            WindowBinaryMask.doubleRedPixels = new double[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.doubleGreenPixels = new double[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.doubleBluePixels = new double[WindowBinaryMask.height, WindowBinaryMask.width];
            //AfterCalcPointClass = new int[WinFrame.height, WinFrame.width];

            int M = (WindowFrame.width - 1) / 2, N = (WindowFrame.height - 1) / 2;
            for (int i = Center_Y - N, c = 0; i < ColoredFrame.height && i < (Center_Y + N) && c < WindowFrame.height; i++, c++)
            {
                if (i < 0) i = 0;
                for (int j = Center_X - M, k = 0; j < ColoredFrame.width && j < (Center_X + M) && k < WindowFrame.width; j++, k++)
                {
                    if (j < 0) j = 0;
                    WindowFrame.byteRedPixels[c, k] = ColoredFrame.byteRedPixels[i, j];
                    WindowFrame.byteGreenPixels[c, k] = ColoredFrame.byteGreenPixels[i, j];
                    WindowFrame.byteBluePixels[c, k] = ColoredFrame.byteBluePixels[i, j];

                    WindowBinaryMask.byteRedPixels[c, k] = BinaryMask.byteRedPixels[i, j];
                    WindowBinaryMask.byteGreenPixels[c, k] = BinaryMask.byteGreenPixels[i, j];
                    WindowBinaryMask.byteBluePixels[c, k] = BinaryMask.byteBluePixels[i, j];

                    WindowFrame.doubleRedPixels[c, k] = ColoredFrame.doubleRedPixels[i, j];
                    WindowFrame.doubleGreenPixels[c, k] = ColoredFrame.doubleGreenPixels[i, j];
                    WindowFrame.doubleBluePixels[c, k] = ColoredFrame.doubleBluePixels[i, j];

                    WindowBinaryMask.doubleRedPixels[c, k] = BinaryMask.doubleRedPixels[i, j];
                    WindowBinaryMask.doubleGreenPixels[c, k] = BinaryMask.doubleGreenPixels[i, j];
                    WindowBinaryMask.doubleBluePixels[c, k] = BinaryMask.doubleBluePixels[i, j];
                }
            }
            WindowFrame.BmpImage = new Bitmap(WindowFrame.width, WindowFrame.height);
            BitmapData bmpData = WindowFrame.BmpImage.LockBits(new Rectangle(0, 0, WindowFrame.width, WindowFrame.height), System.Drawing.Imaging.ImageLockMode.ReadWrite, WindowFrame.BmpImage.PixelFormat);
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                int space = bmpData.Stride - WindowFrame.width * 3;
                for (int i = 0; i < WindowFrame.height; i++)
                {
                    for (int j = 0; j < WindowFrame.width; j++)
                    {
                        p[0] = WindowFrame.byteBluePixels[i, j];
                        p[1] = WindowFrame.byteGreenPixels[i, j];
                        p[2] = WindowFrame.byteRedPixels[i, j];
                        p += 3;
                    }
                    p += space;
                }
            }
            WindowFrame.BmpImage.UnlockBits(bmpData);

            WindowFrame.EmguRgbImage = new Image<Bgr, byte>(WindowFrame.BmpImage);
            WindowFrame.EmguLabImage = WindowFrame.EmguRgbImage.Convert<Lab, byte>();
            WindowFrame.IplImageRGB = (IplImage)cvtools.ConvertPtrToStructure(WindowFrame.EmguRgbImage.Ptr, typeof(IplImage));
            WindowFrame.IplImageLab = (IplImage)cvtools.ConvertPtrToStructure(WindowFrame.EmguLabImage.Ptr, typeof(IplImage));

            string Nw = "FrameTest" + Counter.ToString() + ".bmp";
            Counter++;
            string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + Nw;

            WindowFrame.BmpImage.Save(Pw, ImageFormat.Bmp);
            WindowClassifier = new Classifier();
        }
    }
}
