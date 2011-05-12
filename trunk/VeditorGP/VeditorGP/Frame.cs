using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using openCV;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.Structure;
using System.Drawing.Imaging;

namespace VeditorGP
{
    class Frame
    {
        #region Variables and Constructors
        public List<Window> FrameWindows;
        public int width;
        public int height;
        public byte[,] byteRedPixels;
        public byte[,] byteGreenPixels;
        public byte[,] byteBluePixels;
        public double[,] doubleRedPixels;
        public double[,] doubleGreenPixels;
        public double[,] doubleBluePixels;
        public IplImage IplImageLab, IplImageRGB;
        public Image<Lab, byte> EmguLabImage;
        public Image<Bgr, byte> EmguRgbImage;
        public Bitmap BmpImage;
        public Frame() { }
        #endregion

        #region New - Save to disk
        public void InitializeFrame(Bitmap _BMPImage)
        {
            BmpImage = _BMPImage; 
            width = BmpImage.Width;
            height = BmpImage.Height;
            byteRedPixels = new byte[height, width];
            byteGreenPixels = new byte[height, width];
            byteBluePixels = new byte[height, width];
            doubleRedPixels = new double[height, width];
            doubleGreenPixels = new double[height, width];
            doubleBluePixels = new double[height, width];
            BitmapData bmpData = BmpImage.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, BmpImage.PixelFormat);
            FillFrameRGB(bmpData);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    doubleRedPixels[i, j] = byteRedPixels[i, j];
                    doubleGreenPixels[i, j] = byteGreenPixels[i, j];
                    doubleBluePixels[i, j] = byteBluePixels[i, j];
                }
            EmguRgbImage = new Image<Bgr, byte>(BmpImage);
            EmguLabImage = EmguRgbImage.Convert<Lab, byte>();
            IplImageRGB = (IplImage)cvtools.ConvertPtrToStructure(EmguRgbImage.Ptr, typeof(IplImage));
            IplImageLab = (IplImage)cvtools.ConvertPtrToStructure(EmguLabImage.Ptr, typeof(IplImage));
        }
        void FillFrameRGB(BitmapData bmpData)
        {
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                if (BmpImage.PixelFormat == PixelFormat.Format64bppArgb || BmpImage.PixelFormat == PixelFormat.Format64bppPArgb)
                {
                    int space = bmpData.Stride - width * 8;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            byteBluePixels[i, j] = p[0];
                            byteGreenPixels[i, j] = p[1];
                            byteRedPixels[i, j] = p[2];
                            p += 8;
                        }
                        p += space;
                    }
                }
                if (BmpImage.PixelFormat == PixelFormat.Format32bppArgb || BmpImage.PixelFormat == PixelFormat.Format32bppRgb)
                {
                    int space = bmpData.Stride - width * 4;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            byteBluePixels[i, j] = p[0];
                            byteGreenPixels[i, j] = p[1];
                            byteRedPixels[i, j] = p[2];
                            p += 4;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    int space = bmpData.Stride - width * 3;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            byteBluePixels[i, j] = p[0];
                            byteGreenPixels[i, j] = p[1];
                            byteRedPixels[i, j] = p[2];
                            p += 3;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format16bppRgb555)
                {
                    int space = bmpData.Stride - width * 2;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            byteBluePixels[i, j] = p[0];
                            byteGreenPixels[i, j] = p[1];
                            byteRedPixels[i, j] = p[2];
                            p += 2;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    int space = bmpData.Stride - width;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            byteBluePixels[i, j] = byteGreenPixels[i, j] = byteRedPixels[i, j] = p[0];
                            p++;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format4bppIndexed)
                {
                    int space = bmpData.Stride - ((width / 2) + (width % 2));
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            int e = ((j + 1) % 2 == 0) ? p[0] >> 4 : p[0] & 0x0F;
                            byteBluePixels[i, j] = byteGreenPixels[i, j] = byteRedPixels[i, j] = p[e];
                            p += ((j + 1) % 2);
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format1bppIndexed)
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            byte* ptr = (byte*)bmpData.Scan0 + (i * bmpData.Stride) + (j / 8);
                            byte b = *ptr;
                            byte mask = Convert.ToByte(0x80 >> (j % 8));
                            if ((b & mask) != 0)
                                byteBluePixels[i, j] = byteGreenPixels[i, j] = byteRedPixels[i, j] = 255;
                            else
                                byteBluePixels[i, j] = byteGreenPixels[i, j] = byteRedPixels[i, j] = 0;
                        }
                    }
                }
            }
            BmpImage.UnlockBits(bmpData);
        }
        #endregion
    }
}
