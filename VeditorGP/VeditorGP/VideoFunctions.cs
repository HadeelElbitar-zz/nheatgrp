using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using openCV;
using System.Drawing;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AviFile;
using System.IO;

namespace VeditorGP
{
    class VideoFunctions
    {
        #region Variables and Constructor
        Frame CurrentFrame, PreviousFrame;
        public List<Point> Upper, Lower;
        public double FramePerSecond;
        public Frame InitialSegmentationBinaryFrame, InitialFrame, InitialContourFrame;
        int WindowWidth = 30, WindowHeight = 30;
        static Capture Video, DrMostafaVideo;
        static int CutOutFrameCount = 0;
        string NewVideoFilePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        public string NewVideoPath;
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
            NewVideoFilePath += "\\NewVideoFramesFile.txt";
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
            FileStream FS = new FileStream(NewVideoFilePath, FileMode.Truncate);
            StreamWriter SW = new StreamWriter(FS);
            SW.Close();
            FS.Close();
            return CurrentFrame;
        }
        int OpenFrame()
        {
            Image<Bgr, byte> Temp;
            try
            {
                Temp = Video.QuerySmallFrame();
            }
            catch
            {
                return -1;
            }
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
                    CurrentFrame.byteRedPixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 2];
                    CurrentFrame.byteGreenPixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 1];
                    CurrentFrame.byteBluePixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 0];
                    CurrentFrame.doubleRedPixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 2];
                    CurrentFrame.doubleGreenPixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 1];
                    CurrentFrame.doubleBluePixels[i, j] = CurrentFrame.EmguRgbImage.Data[i, j, 0];
                    //CurrentFrame.BmpImage.SetPixel(j, i, Color.FromArgb(CurrentFrame.byteRedPixels[i, j], CurrentFrame.byteGreenPixels[i, j], CurrentFrame.byteBluePixels[i, j]));
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
                        p[0] = CurrentFrame.byteBluePixels[i, j];
                        p[1] = CurrentFrame.byteGreenPixels[i, j];
                        p[2] = CurrentFrame.byteRedPixels[i, j];
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
            InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, Upper[0], InitialContourFrame));
            //  InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, Upper[1], InitialContourFrame));
            int index = 0;// count = Upper.Count;
            double Distance = 0;
            int upperCount = Upper.Count - 1;
            for (int i = 1; i < Upper.Count - 1; i++)
            {
                Distance = Math.Sqrt(Math.Pow((Upper[i].X - Upper[index].X), 2) + Math.Pow((Upper[i].Y - Upper[index].Y), 2));
                //if (Math.Floor(Distance) > 20)
                //{
                //    Upper.RemoveAt(i);
                //    //index = i;
                //    i--;
                //}
                if (Math.Floor(Distance) >= 10)
                {
                    InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, new Point(Upper[i].X, Upper[i].Y), InitialContourFrame));
                    index = i;
                }
                if (index < 0)
                    break;
            }
            index = Lower.Count - 3;
            InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, Lower[(index + 1)], InitialContourFrame));
            InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, Lower[(index + 2)], InitialContourFrame));
            //  InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, Lower[index], InitialContourFrame));
            for (int i = index - 1; i > 0; i--)
            {
                Distance = Math.Sqrt(Math.Pow((Lower[i].X - Lower[index].X), 2) + Math.Pow((Lower[i].Y - Lower[index].Y), 2));
                //if (Math.Floor(Distance) > 20)
                //{
                //      Lower.RemoveAt(i);
                //      i++;
                //    //index = i;
                //}
                if (Math.Floor(Distance) >= 10)
                {
                    InitialFrame.FrameWindows.Add(new Window(WindowWidth, WindowHeight, InitialFrame, InitialSegmentationBinaryFrame, new Point(Lower[i].X, Lower[i].Y), InitialContourFrame));
                    index = i;
                }
                if (index == Lower.Count)
                    break;
            }
            CurrentFrame = InitialFrame;
        }
        public void TrainClassifiers()
        {
            foreach (Window item in InitialFrame.FrameWindows)
            {
                item.WindowClassifier.Train();
                item.ClassificationMask = item.WindowClassifier.OurGMM();
                item.CalculateModels();
            }
            GenerateCutOutFrame(InitialFrame);
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
        public void PropagateFrame(bool ShowWindow, PictureBox FrameBox)
        {
            while (GetNextFrame() != null)
            {
                GetSurfPoints();
                GetOpticalFlow();
                WarpWindows();// Update Shape Model.
                if (ShowWindow)
                    ShowWin(FrameBox);
                ClassifyNewFrame();
                GenerateCutOutFrame(CurrentFrame);
            }
            CreateNewVideo();
        }
        void ClassifyNewFrame()
        {
            int i = -1;
            foreach (Window item in CurrentFrame.FrameWindows)
            {
                item.UpdatedBinaryFrame = item.WindowClassifier.OurGMM(); //P(x)
                item.ForegroundProbability = item.WindowClassifier.MyWindow.ForegroundProbability;
                item.CalculateModels(PreviousFrame.FrameWindows[++i]);
            }
        }
        void WarpWindows()
        {
            //hena na2es el averging
            CurrentFrame.FrameWindows = new List<Window>();
            foreach (Window item in PreviousFrame.FrameWindows)
                CurrentFrame.FrameWindows.Add(new Window(new PointF((float)item.Center_X + (float.Parse(FlowX.Data[(int)item.Center_Y, (int)item.Center_X, 0].ToString())), (float)item.Center_Y + (float.Parse(FlowY.Data[(int)item.Center_Y, (int)item.Center_X, 0].ToString()))), CurrentFrame, item, FlowX, FlowY));
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
            //OpticalFlowObject.ComputeDenseOpticalFlow(PreviousFrame, WarpedFrame);
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

        #region Generate New Video Frames
        void GenerateCutOutFrame(Frame OldFrame)
        {
            #region Initializations
            Frame NewFrame = new Frame();
            NewFrame.width = OldFrame.width;
            NewFrame.height = OldFrame.height;
            NewFrame.byteRedPixels = new byte[NewFrame.height, NewFrame.width];
            NewFrame.byteGreenPixels = new byte[NewFrame.height, NewFrame.width];
            NewFrame.byteBluePixels = new byte[NewFrame.height, NewFrame.width];
            #endregion

            #region Loop
            Bitmap NewImage = new Bitmap(NewFrame.width, NewFrame.height);
            Bitmap NewImageBinary = new Bitmap(NewFrame.width, NewFrame.height);
            foreach (Window item in OldFrame.FrameWindows)
            {
                int M = (item.WindowFrame.width - 1) / 2, N = (item.WindowFrame.height - 1) / 2;
                for (int i = ((int)item.Center_Y - M), c = 0; i <= (item.Center_Y + M); i++, c++)
                {
                    if (i < 0) i = 0;
                    for (int j = ((int)item.Center_X - N), k = 0; j <= (item.Center_X + N); j++, k++)
                    {
                        if (j < 0) j = 0;
                        if (item.ClassificationMask[c, k] == 255)
                        {
                            NewImage.SetPixel(j, i, Color.FromArgb(item.WindowFrame.byteRedPixels[c, k], item.WindowFrame.byteGreenPixels[c, k], item.WindowFrame.byteBluePixels[c, k]));
                            NewImageBinary.SetPixel(j, i, Color.White);
                        }
                    }
                }
            }
            #endregion

            #region Fill Binary Mask
            FloodFiller flood = new FloodFiller();
            flood.FloodFill(ref NewImageBinary, new Point(116, 110));
            #endregion

            #region Anding
            for (int i = 0; i < NewFrame.height; i++)
                for (int j = 0; j < NewFrame.width; j++)
                    NewImage.SetPixel(j, i, Color.FromArgb(OldFrame.byteRedPixels[i, j] & (byte)(NewImageBinary.GetPixel(j, i).R),
                        OldFrame.byteGreenPixels[i, j] & (byte)(NewImageBinary.GetPixel(j, i).G),
                        OldFrame.byteBluePixels[i, j] & (byte)(NewImageBinary.GetPixel(j, i).B)));
            #endregion

            #region Test Saving Boundary Image
            string Pw;
            Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Filled Colored Windows Integration " + CutOutFrameCount + ".bmp";
            NewImage.Save(Pw, ImageFormat.Bmp);
            StreamWriter SWrite = new StreamWriter(NewVideoFilePath, true);
            SWrite.WriteLine(Pw);
            SWrite.Close();
            //Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Filled Binary Windows Integration " + CutOutFrameCount + ".bmp";
            //NewImageBinary.Save(Pw, ImageFormat.Bmp);
            CutOutFrameCount++;
            #endregion
        }
        #endregion

        #region Show Windows
        public void ShowWin(PictureBox FrameBox)
        {
            Pen myPen = new System.Drawing.Pen(System.Drawing.Color.RoyalBlue);
            Graphics formGraphics = FrameBox.CreateGraphics();
            foreach (Window w in CurrentFrame.FrameWindows)
            {
                float X = w.Center_X;
                float Y = w.Center_Y;
                formGraphics.DrawRectangle(myPen, X - 15, Y - 15, 30, 30);
            }
            myPen.Dispose();
            formGraphics.Dispose();
        }
        #endregion

        #region Generate New Video
        void CreateNewVideo()
        {
            StreamReader SR = new StreamReader(NewVideoFilePath);
            string Line = SR.ReadLine();
            NewVideoPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\New Video.avi";
            AviManager aviManager = new AviManager(NewVideoPath, false);
            Bitmap bmp = (Bitmap)Image.FromFile(Line);
            VideoStream aviStream = aviManager.AddVideoStream(false, FramePerSecond, bmp);
            Bitmap bitmap;
            int count = 0;
            while ((Line = SR.ReadLine()) != null)
            {
                if (Line.Trim().Length > 0)
                {
                    bitmap = (Bitmap)Bitmap.FromFile(Line);
                    aviStream.AddFrame(bitmap);
                    bitmap.Dispose();
                    count++;
                }
            }
            SR.Close();
            aviManager.Close();
        }
        #endregion
    }
}
