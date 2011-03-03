using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using openCV;

namespace GraduationProject
{
    public class Frame
    {
        #region Variables
        public int width;
        public int height;
        public string Path;
        public PictureBox FrameBox;
        public byte[,] redPixels;
        public byte[,] greenPixels;
        public byte[,] bluePixels;
        IplImage Lab;
        #endregion

        #region Constructors
        public Frame()
        {

        }
        public Frame(int _width, int _height, PictureBox _frameBox, byte[,] _redPixels, byte[,] _greenPixels, byte[,] _bluePixels, IplImage _Lab, string _Path)
        {
            Path = _Path;
            width = _width;
            height = _height;
            FrameBox = _frameBox;
            redPixels = _redPixels;
            greenPixels = _greenPixels;
            bluePixels = _bluePixels;
            Lab = _Lab;
        }
        public Frame(Frame pic)
        {
            width = pic.width;
            height = pic.height;
            FrameBox = pic.FrameBox;
            redPixels = new byte[height, width];
            greenPixels = new byte[height, width];
            bluePixels = new byte[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    redPixels[i, j] = pic.redPixels[i, j];
                    greenPixels[i, j] = pic.greenPixels[i, j];
                    bluePixels[i, j] = pic.bluePixels[i, j];
                }
            }
            Lab = pic.Lab;
        }
        #endregion

        #region OpenFrame
        public void OpenFrame(string PicturePath, PictureBox picBox)
        {
            Bitmap Bmp = new Bitmap(PicturePath);
            width = Bmp.Width;
            height = Bmp.Height;
            Path = PicturePath;
            redPixels = new byte[height, width];
            greenPixels = new byte[height, width];
            bluePixels = new byte[height, width];
            BitmapData bmpData = Bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, Bmp.PixelFormat);

            #region RGB
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                if (Bmp.PixelFormat == PixelFormat.Format64bppArgb || Bmp.PixelFormat == PixelFormat.Format64bppPArgb)
                {
                    int space = bmpData.Stride - width * 8;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
                            p += 8;
                        }
                        p += space;
                    }
                }
                if (Bmp.PixelFormat == PixelFormat.Format32bppArgb || Bmp.PixelFormat == PixelFormat.Format32bppRgb)
                {
                    int space = bmpData.Stride - width * 4;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
                            p += 4;
                        }
                        p += space;
                    }
                }
                else if (Bmp.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    int space = bmpData.Stride - width * 3;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
                            p += 3;
                        }
                        p += space;
                    }
                }
                else if (Bmp.PixelFormat == PixelFormat.Format16bppRgb555)
                {
                    int space = bmpData.Stride - width * 2;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
                            p += 2;
                        }
                        p += space;
                    }
                }
                else if (Bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    int space = bmpData.Stride - width;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = p[0];
                            p++;
                        }
                        p += space;
                    }
                }
                else if (Bmp.PixelFormat == PixelFormat.Format4bppIndexed)
                {
                    int space = bmpData.Stride - ((width / 2) + (width % 2));
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            int e = ((j + 1) % 2 == 0) ? p[0] >> 4 : p[0] & 0x0F;
                            bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = p[e];
                            p += ((j + 1) % 2);
                        }
                        p += space;
                    }
                }
                else if (Bmp.PixelFormat == PixelFormat.Format1bppIndexed)
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            byte* ptr = (byte*)bmpData.Scan0 + (i * bmpData.Stride) + (j / 8);
                            byte b = *ptr;
                            byte mask = Convert.ToByte(0x80 >> (j % 8));
                            if ((b & mask) != 0)
                                bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = 255;
                            else
                                bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = 0;
                        }
                    }
                }
            }
            Bmp.UnlockBits(bmpData);
            #endregion
        }
        #endregion

        #region Color Space Conversion
        public void RGB2Lab(string PicturePath)
        {
            unsafe
            {
                IplImage dest = new IplImage();
                IplImage src = cvlib.CvLoadImage(PicturePath, cvlib.CV_LOAD_IMAGE_UNCHANGED);
                cvlib.CvCvtColor(ref src, ref dest, cvlib.CV_RGB2Lab);
                cvlib.CvCopy(ref dest, ref Lab);
            }
        }
        public void Lab2RGB(string PicturePath)
        {
            unsafe
            {
                IplImage dest = new IplImage();
                IplImage src = new IplImage();
                cvlib.CvCopy(ref Lab, ref src);
                cvlib.CvCvtColor(ref src, ref dest, cvlib.CV_Lab2RGB);
            }
        }
        #endregion
    }

}