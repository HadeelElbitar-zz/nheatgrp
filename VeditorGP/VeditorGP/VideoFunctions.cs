using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using openCV;
using System.Drawing;
using Emgu.CV.Structure;
using System.Drawing.Imaging;

namespace VeditorGP
{
    class VideoFunctions
    {
        #region Variables and Constructor
        Frame CurrentFrame, PreviousFrame;
        public double FramePerSecond;
        public Frame InitialSegmentationBinaryFrame, InitialFrame, InitialContourFrame, FirstFrame;
        int WindowWidth = 30, WindowHeight = 30, WindowSize = 900;
        static Capture Video , DrMostafaVideo;
        public List<Point> ConnectedContour;
        SURF SurfObject;
        OurOpticalFlow OpticalFlowObject;
        Bitmap SurfResult;
        string VideoPath;
        public VideoFunctions()
        {

        }
        #endregion

        #region Import Video
        public Frame OpenVideo(string Path)
        {
            Video = new Capture(Path);
            VideoPath = Path;
            FramePerSecond = Video.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS);
            DrMostafaVideo = new Capture(Path);
            OpenFrame();
            return CurrentFrame;
        }
        int OpenFrame()
        {
            Image<Bgr, byte> Temp = Video.QuerySmallFrame();
            if (Temp == null)
                return -1;
            CurrentFrame = new Frame();
            CurrentFrame.EmguRgbImage = Temp;
            CurrentFrame.width = CurrentFrame.EmguRgbImage.Cols;
            CurrentFrame.height = CurrentFrame.EmguRgbImage.Rows;
            CurrentFrame.EmguLabImage = CurrentFrame.EmguRgbImage.Convert<Lab, byte>();
            CurrentFrame.BmpImage = new Bitmap(CurrentFrame.width, CurrentFrame.height, PixelFormat.Format24bppRgb);
            CurrentFrame.IplImageRGB = (IplImage)cvtools.ConvertPtrToStructure(CurrentFrame.EmguRgbImage.Ptr, typeof(IplImage));
            CurrentFrame.IplImageLab = (IplImage)cvtools.ConvertPtrToStructure(CurrentFrame.EmguLabImage.Ptr, typeof(IplImage));
            CurrentFrame.byteRedPixels = new byte[CurrentFrame.height, CurrentFrame.width];
            CurrentFrame.byteGreenPixels = new byte[CurrentFrame.height, CurrentFrame.width];
            CurrentFrame.byteBluePixels = new byte[CurrentFrame.height, CurrentFrame.width];
            CurrentFrame.doubleRedPixels = new double[CurrentFrame.height, CurrentFrame.width];
            CurrentFrame.doubleGreenPixels = new double[CurrentFrame.height, CurrentFrame.width];
            CurrentFrame.doubleBluePixels = new double[CurrentFrame.height, CurrentFrame.width];
            for (int i = 0; i < CurrentFrame.height; i++)
            {
                for (int j = 0; j < CurrentFrame.width; j++)
                {
                    CurrentFrame.byteRedPixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 0];
                    CurrentFrame.byteGreenPixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 1];
                    CurrentFrame.byteBluePixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 2];
                    CurrentFrame.doubleRedPixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 0];
                    CurrentFrame.doubleGreenPixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 1];
                    CurrentFrame.doubleBluePixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 2];
                }
            }
            BitmapData bmpData = CurrentFrame.BmpImage.LockBits(new Rectangle(0, 0, CurrentFrame.width, CurrentFrame.height), System.Drawing.Imaging.ImageLockMode.ReadWrite, CurrentFrame.BmpImage.PixelFormat);
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                int space = bmpData.Stride - CurrentFrame.width * 3;
                for (int i = 0; i < CurrentFrame.height; i++)
                {
                    for (int j = 0; j < CurrentFrame.width; j++)
                    {
                        p[0] = CurrentFrame.byteRedPixels[i, j];
                        p[1] = CurrentFrame.byteGreenPixels[i, j];
                        p[2] = CurrentFrame.byteBluePixels[i, j];
                        p += 3;
                    }
                    p += space;
                }
            }
            CurrentFrame.BmpImage.UnlockBits(bmpData);
            return 0;
        }
        #endregion

        #region Initialize Winodws and Train Classifiers
        public void SetInitialWindowsArroundContour()
        {
            InitialFrame.FrameWindows = new List<Window>();
            InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, ConnectedContour[0], InitialContourFrame));
            int newIndex = 0, index = 0, length = ConnectedContour.Count;
            double Distance, Temp;
            int LoopCounter = 1, count = ConnectedContour.Count; // WindowCount = ConnectedContour.Count / (WindowSize - OverLappingArea);
            Distance = 0;
            for (int i = LoopCounter; i < ConnectedContour.Count; i++)
                for (int j = 0; j < ConnectedContour.Count; j++)
                {
                    Temp = Distance;
                    Distance = Math.Sqrt(Math.Pow((ConnectedContour[j].X - ConnectedContour[index].X), 2) + Math.Pow((ConnectedContour[j].Y - ConnectedContour[index].Y), 2));
                    if (Math.Floor(Distance) == 20)
                    {
                        newIndex = ConnectedContour.IndexOf(ConnectedContour[j]);
                        ConnectedContour.Remove(ConnectedContour[j]);
                        LoopCounter = ++j;
                        InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, ConnectedContour[newIndex], InitialContourFrame));
                        index = newIndex;
                        break;
                    }
                }
        }
        public void TrainClassifiers()
        {
            foreach (Window item in InitialFrame.FrameWindows)
            {
                item.WindowClassifier.Train();
                item.CalculateModels();
            }
        }
        #endregion

        #region Propagation
        public Frame GetNextFrame()
        {
            PreviousFrame = CurrentFrame;
            if (OpenFrame() == -1)
                return null;
            return CurrentFrame;
        }
        public void PropagateFrame()
        {
            GetNextFrame();
            GetSurfPoints();
            GetOpticalFlow();
        }
        void GetSurfPoints()
        {
            SurfObject = new SURF();
            SurfResult = SurfObject.GetSIFTpoints(PreviousFrame, CurrentFrame);
        }
        void GetOpticalFlow()
        {
            OpticalFlowObject = new OurOpticalFlow();
        }
        #endregion

        #region View Frames for Dr.Mostafa
        public Bitmap GetNewFrame(ref int Success)
        {
            Bitmap Result;
            Image<Bgr, byte> Frame = DrMostafaVideo.QueryFrame();
            if (Frame == null)
            {
                Success = -1;
                return null;
            }
            IplImage TempFrame = (IplImage)cvtools.ConvertPtrToStructure(Frame.Ptr, typeof(IplImage));
            Result = (Bitmap)TempFrame;
            return Result;
        }
        #endregion
    }
}
