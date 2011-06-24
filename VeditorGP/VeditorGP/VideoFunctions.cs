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
        Frame CurrentFrame, PreviousFrame, TempCurrentFrame;
        public List<Point> Upper, Lower;
        public double FramePerSecond;
        public Frame InitialSegmentationBinaryFrame, InitialFrame, InitialContourFrame, FirstFrame;
        int WindowWidth = 30, WindowHeight = 30, WindowSize = 900;
        static Capture Video, DrMostafaVideo;
        public List<Point> ConnectedContour;
        SURF SurfObject;
        OurOpticalFlow OpticalFlowObject;
        Image<Gray, Byte> WarpedFrame;
        Image<Gray, Single> FlowX, FlowY;
        string VideoPath;
        public VideoFunctions()
        {
            Upper = new List<Point>();
            Lower = new List<Point>();
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
            InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, Upper[1], InitialContourFrame));
            int index = 1;// count = Upper.Count;
            double Distance = 0;
            for (int i = 2; i < Upper.Count - 1; i++)
            {
                Distance = Math.Sqrt(Math.Pow((Upper[i].X - Upper[index].X), 2) + Math.Pow((Upper[i].Y - Upper[index].Y), 2));
                if (Math.Floor(Distance) > 20)
                {
                    Upper.RemoveAt(i);
                    i--;
                }
                if (Math.Floor(Distance) == 20)
                {
                    InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, new Point(Upper[i].X, Upper[i].Y), InitialContourFrame));
                    index = i;
                }
            }
            index = Lower.Count - 2;
            InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, Lower[index], InitialContourFrame));
            for (int i = index - 1; i > 0; i--)
            {
                Distance = Math.Sqrt(Math.Pow((Lower[i].X - Lower[index].X), 2) + Math.Pow((Lower[i].Y - Lower[index].Y), 2));
                if (Math.Floor(Distance) > 20)
                {
                    Lower.RemoveAt(i);
                    i++;
                }
                if (Math.Floor(Distance) == 20)
                {
                    InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, new Point(Lower[i].X, Lower[i].Y), InitialContourFrame));
                    index = i;
                }
            }
        }
        public void TrainClassifiers()
        {
            CurrentFrame = InitialFrame;
            foreach (Window item in InitialFrame.FrameWindows)
            {
                item.WindowClassifier.Train();
                item.WindowClassifier.OurGMM();
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
            WarpWindows();
            ClassifyNewFrame();
        }
        void ClassifyNewFrame()
        {
            foreach (Window item in CurrentFrame.FrameWindows)
            {
                item.WindowClassifier.OurGMM();
                //item.CalculateModels();
            }
        }
        void WarpWindows()
        {
            //hena na2es el averging
            CurrentFrame.FrameWindows = new List<Window>();
            foreach (Window item in PreviousFrame.FrameWindows)
                CurrentFrame.FrameWindows.Add(new Window(new Point(item.Center_X + int.Parse(FlowX.Data[item.Center_Y, item.Center_X, 0].ToString()), item.Center_Y + int.Parse(FlowY.Data[item.Center_Y, item.Center_X, 0].ToString())), CurrentFrame, item));
        }
        void GetSurfPoints()
        {
            SurfObject = new SURF();
            WarpedFrame = new Image<Gray, byte>(CurrentFrame.BmpImage.Size);
            WarpedFrame = SurfObject.GetSIFTpoints(PreviousFrame, CurrentFrame);
        }
        void GetOpticalFlow()
        {
            OpticalFlowObject = new OurOpticalFlow();
            List<Image<Gray, Single>> Result = OpticalFlowObject.OpticalFlowWorker(PreviousFrame, WarpedFrame);
            FlowX = Result[0];
            FlowY = Result[1];
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
