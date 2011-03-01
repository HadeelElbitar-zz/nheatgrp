using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using openCV;
using Emgu.CV.UI;
using System.Runtime.InteropServices;

namespace GraduationProject
{
    class ContourFunctions
    {
        private List<CvPoint> ContourPoints = new List<CvPoint>();
        public List<Window> WindowList = new List<Window>();
        private int WindowWidth, WindowHeight;
        public ContourFunctions(int _WindowWidth, int _WindowHeight) 
        {
            WindowWidth = _WindowWidth;
            WindowHeight = _WindowHeight;
        }
        public Frame GetContour(Frame _Frame)
        {
            Frame NewImage = new Frame();
            NewImage.width = _Frame.width;
            NewImage.height = _Frame.height;
            NewImage.redPixels = new byte[NewImage.height, NewImage.width];
            NewImage.greenPixels = new byte[NewImage.height, NewImage.width];
            NewImage.bluePixels = new byte[NewImage.height, NewImage.width];

            NewImage.L = new double[NewImage.height, NewImage.width];
            NewImage.A = new double[NewImage.height, NewImage.width];
            NewImage.B = new double[NewImage.height, NewImage.width];
            int levels = 1;
            CvSeq Sequence = new CvSeq();
            GCHandle Handel;
            IntPtr contours = cvtools.ConvertStructureToPtr(Sequence, out Handel);
            CvMemStorage storage = cvlib.CvCreateMemStorage(0);
            IplImage Img;
            Img = cvlib.CvLoadImage(_Frame.Path, cvlib.CV_LOAD_IMAGE_GRAYSCALE);
            //cvlib.CvNamedWindow("image", cvlib.CV_WINDOW_AUTOSIZE);
            //cvlib.CvShowImage("image", ref Img);
            cvlib.CvFindContours(ref Img, ref storage, ref contours, Marshal.SizeOf(typeof(CvContour)), cvlib.CV_RETR_TREE, cvlib.CV_CHAIN_APPROX_SIMPLE, cvlib.CvPoint(0, 0));
            //int x_1 = cvlib.CvNamedWindow("contours", cvlib.CV_WINDOW_AUTOSIZE);
            IplImage cnt_img = cvlib.CvCreateImage(cvlib.CvSize(500, 500), 8, 3);
            Sequence = (CvSeq)cvtools.ConvertPtrToStructure(contours, typeof(CvSeq));
            cvlib.CvDrawContours(ref cnt_img, ref Sequence, cvlib.CV_RGB(110, 230, 120), cvlib.CV_RGB(0, 255, 0), levels, 1, cvlib.CV_AA, cvlib.CvPoint(0, 0));
            //cvlib.CvShowImage("contours", ref cnt_img);
            #region Convert to Points
            CvPoint point;
            IntPtr currSeqPtr = contours;
            for (; currSeqPtr != IntPtr.Zero; currSeqPtr = Sequence.h_next)
            {
                Sequence = (CvSeq)cvtools.ConvertPtrToStructure(currSeqPtr, typeof(CvSeq));
                for (int i = 0; i < Sequence.total; i++)
                {
                    IntPtr pointPtr = CvInvoke.cvGetSeqElem(currSeqPtr, i);
                    point = (CvPoint)cvtools.ConvertPtrToStructure(pointPtr, typeof(CvPoint));
                    ContourPoints.Add(point);
                }
            }
            #endregion
            FrameFunctions FFn = new FrameFunctions();
            FFn.CopyFrameData(cnt_img, ref NewImage);
            SetWindows(_Frame);
            return NewImage;
        }
        public List<CvPoint> _ContourPoints
        {
            get
            {
                return ContourPoints;
            }
        }
        private void SetWindows(Frame _Frame)
        {
            Window Win;
            WindowList.Add(Win = new Window(WindowWidth, WindowHeight, _Frame, ContourPoints[0]));
            int WinSize, OverLappingArea, newIndex, index = 0;
            CvPoint newPoint;
            while (true)
            {
                WinSize = WindowWidth;
                OverLappingArea = (int)Math.Ceiling((double)(WinSize / 3));
                newPoint = new CvPoint(ContourPoints[index].x + WinSize - OverLappingArea, ContourPoints[index].y);
                newIndex = ContourPoints.IndexOf(newPoint);
                if (newIndex == -1) //not on the same y
                {
                    WinSize = WindowHeight;
                    OverLappingArea = (int)Math.Ceiling((double)(WinSize / 3));
                    newPoint = new CvPoint(ContourPoints[index].x, ContourPoints[index].y + WinSize - OverLappingArea);
                    newIndex = ContourPoints.IndexOf(newPoint);
                    if (newIndex == -1)  //not on the same x
                    {
                        WinSize = 2 * (int)(Math.Sqrt(Math.Pow(WindowHeight / 2, 2) + Math.Pow(WindowWidth / 2, 2)));
                        OverLappingArea = (int)Math.Ceiling((double)(WinSize / 3));
                        newPoint = new CvPoint(ContourPoints[index].x + WinSize - OverLappingArea, ContourPoints[index].y + WinSize - OverLappingArea);
                        newIndex = ContourPoints.IndexOf(newPoint);
                        if (newIndex == -1)
                        {
                            newPoint = new CvPoint(ContourPoints[index].x - (WinSize - OverLappingArea), ContourPoints[index].y - (WinSize - OverLappingArea));
                            newIndex = ContourPoints.IndexOf(newPoint);
                            if (newIndex == -1)
                            {
                                newPoint = new CvPoint(ContourPoints[index].x - (WinSize - OverLappingArea), ContourPoints[index].y + WinSize - OverLappingArea);
                                newIndex = ContourPoints.IndexOf(newPoint);
                                if (newIndex == -1)
                                {
                                    newPoint = new CvPoint(ContourPoints[index].x + WinSize - OverLappingArea, ContourPoints[index].y - (WinSize - OverLappingArea));
                                    newIndex = ContourPoints.IndexOf(newPoint);
                                }
                            }
                        }
                    }
                }
                Win = new Window(WindowWidth, WindowHeight, _Frame, ContourPoints[newIndex]);
                if (WindowList.Contains(Win))
                    break;
                WindowList.Add(Win);
                index = newIndex;
            }
        }
    }
}
