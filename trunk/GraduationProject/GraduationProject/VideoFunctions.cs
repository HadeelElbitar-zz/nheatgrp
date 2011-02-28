using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    Frames.Add(Frame);
                }
            }
            catch { }
            return Frames;
        }
    }
}