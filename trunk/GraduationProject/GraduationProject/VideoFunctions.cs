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
using System.Runtime.InteropServices;

namespace GraduationProject
{
    class VideoFunctions
    {
        static public List<Frame> Frames;
        static public Capture Video;
        public VideoFunctions() { }
        public Frame LoadVideoFrames(string Path)
        {


            Frames = new List<Frame>();
            Video = new Capture(Path);
            Frame Frame = new Frame();
            Frame.RgbImage = Video.QuerySmallFrame();
            Frame.LabImage = Frame.RgbImage.Convert<Lab, byte>();
            Frame.RGB = (IplImage)cvtools.ConvertPtrToStructure(Frame.RgbImage.Ptr, typeof(IplImage));
            Frame.Lab = (IplImage)cvtools.ConvertPtrToStructure(Frame.LabImage.Ptr, typeof(IplImage));
            Frame.width = Frame.RgbImage.Cols;
            Frame.height = Frame.RgbImage.Rows;
            Frame.redPixels = new byte[Frame.height, Frame.width];
            Frame.greenPixels = new byte[Frame.height, Frame.width];
            Frame.bluePixels = new byte[Frame.height, Frame.width];
            for (int i = 0; i < Frame.height; i++)
            {
                for (int j = 0; j < Frame.width; j++)
                {
                    Frame.redPixels[i, j] = Frame.RgbImage.Data[i, j, 0];
                    Frame.greenPixels[i, j] = Frame.RgbImage.Data[i, j, 1];
                    Frame.bluePixels[i, j] = Frame.RgbImage.Data[i, j, 2];
                }
            }
                    
            Frame.BmpImage = new Bitmap(Frame.width, Frame.height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = Frame.BmpImage.LockBits(new Rectangle(0, 0, Frame.width, Frame.height), System.Drawing.Imaging.ImageLockMode.ReadWrite, Frame.BmpImage.PixelFormat);
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                int space = bmpData.Stride - Frame.width * 3;
                for (int i = 0; i < Frame.height; i++)
                {
                    for (int j = 0; j < Frame.width; j++)
                    {
                        p[0] = Frame.bluePixels[i, j];
                        p[1] = Frame.greenPixels[i, j];
                        p[2] = Frame.redPixels[i, j];
                        p += 3;
                    }
                    p += space;
                }
            }
            Frame.BmpImage.UnlockBits(bmpData);
            Frames.Add(Frame);
            return Frame;
        }
        public Frame GetNextFrame()
        {
            Frame Frame = new Frame();
            Frame.RgbImage = Video.QuerySmallFrame();
            Frame.LabImage = Frame.RgbImage.Convert<Lab, byte>();
            Frame.RGB = (IplImage)cvtools.ConvertPtrToStructure(Frame.RgbImage.Ptr, typeof(IplImage));
            Frame.Lab = (IplImage)cvtools.ConvertPtrToStructure(Frame.LabImage.Ptr, typeof(IplImage));
            Frame.width = Frame.RgbImage.Cols;
            Frame.height = Frame.RgbImage.Rows;
            Frame.redPixels = new byte[Frame.height, Frame.width];
            Frame.greenPixels = new byte[Frame.height, Frame.width];
            Frame.bluePixels = new byte[Frame.height, Frame.width];
            for (int i = 0; i < Frame.height; i++)
            {
                for (int j = 0; j < Frame.width; j++)
                {
                    Frame.redPixels[i, j] = Frame.RgbImage.Data[i, j, 0];
                    Frame.greenPixels[i, j] = Frame.RgbImage.Data[i, j, 1];
                    Frame.bluePixels[i, j] = Frame.RgbImage.Data[i, j, 2];
                }
            }

            Frame.BmpImage = new Bitmap(Frame.width, Frame.height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = Frame.BmpImage.LockBits(new Rectangle(0, 0, Frame.width, Frame.height), System.Drawing.Imaging.ImageLockMode.ReadWrite, Frame.BmpImage.PixelFormat);
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                int space = bmpData.Stride - Frame.width * 3;
                for (int i = 0; i < Frame.height; i++)
                {
                    for (int j = 0; j < Frame.width; j++)
                    {
                        p[0] = Frame.bluePixels[i, j];
                        p[1] = Frame.greenPixels[i, j];
                        p[2] = Frame.redPixels[i, j];
                        p += 3;
                    }
                    p += space;
                }
            }
            Frame.BmpImage.UnlockBits(bmpData);
            Frames.Add(Frame);
            return Frame;
        }
    }
}