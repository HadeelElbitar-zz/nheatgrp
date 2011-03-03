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
    class VideoFunctions
    {
        public VideoFunctions() { }
        public List<Frame> LoadVideoFrames(string Path)
        {
            int Fname = 0;
            List<Frame> Frames = new List<Frame>();
            Capture Video = new Capture(Path);
            Image<Bgr, byte> TempFrame; //= //new Image<Bgr,byte>();
            try
            {
                while (true)
                {
                    Frame Frame = new Frame();
                    TempFrame = Video.QueryFrame();
                    Frame.width = TempFrame.Cols;
                    Frame.height = TempFrame.Rows;
                    Frame.redPixels = new byte[Frame.height, Frame.width];
                    Frame.greenPixels = new byte[Frame.height, Frame.width];
                    Frame.bluePixels = new byte[Frame.height, Frame.width];
                    for (int i = 0; i < Frame.height; i++)
                    {
                        for (int j = 0; j < Frame.width; j++)
                        {
                            Frame.redPixels[i, j] = TempFrame.Data[i, j, 0];
                            Frame.greenPixels[i, j] = TempFrame.Data[i, j, 1];
                            Frame.bluePixels[i, j] = TempFrame.Data[i, j, 2];
                        }
                    }
                    Bitmap bmp = new Bitmap(Frame.width, Frame.height, PixelFormat.Format24bppRgb);
                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, Frame.width, Frame.height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
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
                    bmp.UnlockBits(bmpData);
                    Frame.FName = "Frame" + Fname.ToString() + ".bmp";
                    Fname++;
                    Frame.Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + Frame.FName;
                    bmp.Save(Frame.Path, ImageFormat.Bmp);
                    Frame.RGB2Lab(Frame.Path);
                    Frames.Add(Frame);
                }
            }
            catch { }
            return Frames;
        }

    }
}