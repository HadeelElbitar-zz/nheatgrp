using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using openCV;
using Emgu.CV.UI;
using System.Runtime.InteropServices;

namespace GraduationProject
{
    public class Frame
    {
        #region Variables
        public int width;
        public int height;
        public PictureBox FrameBox;
        public byte[,] redPixels;
        public byte[,] greenPixels;
        public byte[,] bluePixels;
        public IplImage Lab , RGB;
        public Image<Lab, byte> LabImage;
        public Image<Bgr, byte> RgbImage;
        public Bitmap BmpImage;
        #endregion

        #region Constructors
        public Frame()
        {

        }
        public Frame(int _width, int _height, PictureBox _frameBox, byte[,] _redPixels, byte[,] _greenPixels, byte[,] _bluePixels, IplImage _Lab, IplImage _Rgb , Image<Bgr , byte> IRgb , Image<Lab , byte> ILab, Bitmap _bmp)
        {
            width = _width;
            height = _height;
            FrameBox = _frameBox;
            redPixels = _redPixels;
            greenPixels = _greenPixels;
            bluePixels = _bluePixels;
            Lab = _Lab;
            RGB = _Rgb;
            LabImage = ILab;
            RgbImage = IRgb;
            BmpImage = _bmp;
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
            RGB = pic.RGB;
            LabImage = pic.LabImage;
            RgbImage = pic.RgbImage;
            BmpImage = pic.BmpImage;
        }
        #endregion

        #region OpenFrame
        public void OpenFrame(string PicturePath, PictureBox picBox)
        {
            BmpImage = new Bitmap(PicturePath);
            width = BmpImage.Width;
            height = BmpImage.Height;
            FrameBox = picBox;
            redPixels = new byte[height, width];
            greenPixels = new byte[height, width];
            bluePixels = new byte[height, width];
            BitmapData bmpData = BmpImage.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, BmpImage.PixelFormat);

            #region RGB
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
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
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
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
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
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
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
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
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
                            bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = p[0];
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
                            bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = p[e];
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
                                bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = 255;
                            else
                                bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = 0;
                        }
                    }
                }
            }
            BmpImage.UnlockBits(bmpData);
            #endregion

            RgbImage = new Image<Bgr, byte>(BmpImage);
            LabImage = RgbImage.Convert<Lab, byte>();
            RGB = (IplImage)cvtools.ConvertPtrToStructure(RgbImage.Ptr, typeof(IplImage));
            Lab = (IplImage)cvtools.ConvertPtrToStructure(LabImage.Ptr, typeof(IplImage));
        }
        #endregion

        #region Color Space Conversion
        public void RGB2Lab(string PicturePath)
        {
            //unsafe
            //{
            //    IplImage dest = new IplImage();
            //    IplImage src = cvlib.CvLoadImage(PicturePath, cvlib.CV_LOAD_IMAGE_UNCHANGED);
            //    dest = src;
            //    Lab = dest;
            //    cvlib.CvCvtColor(ref src, ref dest, cvlib.CV_RGB2Lab);
            //    cvlib.CvCopy(ref dest, ref Lab);
            //}
            //cvlib.CvCvtColor(
        }
        public void Lab2RGB(string PicturePath)
        {
            //unsafe
            //{
            //    IplImage dest = new IplImage();
            //    IplImage src = new IplImage();
            //    cvlib.CvCopy(ref Lab, ref src);
            //    cvlib.CvCvtColor(ref src, ref dest, cvlib.CV_Lab2RGB);
            //}
        }
        #endregion
    }

}