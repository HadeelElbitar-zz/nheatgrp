using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using openCV;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VeditorGP
{
    class ContourFunctions
    {
        List<Vector2F> Upper;
        List<Vector2F> Lower;
        public ContourFunctions() { }

        #region Mask Frame and Contour
        public Frame GetBlackAndWhiteContour(CvPoint[] Points, Bitmap BmpImage)
        {
            int width = BmpImage.Width;
            int height = BmpImage.Height;

            IplImage image = cvlib.ToIplImage((Bitmap)BmpImage, true);
            cvlib.CvSetZero(ref image);

            GCHandle Handel;
            IntPtr PointsPtr = cvtools.ConvertStructureToPtr(Points, out Handel);
            cvlib.CvFillPoly(ref image, ref PointsPtr, new int[] { Points.Count() }, 1, cvlib.CV_RGB(255, 255, 255), cvlib.CV_AA, 0);

            //cvlib.CvFillConvexPoly(ref image, ref pts[0], pts.Count(), cvlib.CV_RGB(255, 255, 255), cvlib.CV_AA, 0);
            Frame frame = new Frame();
            frame.InitializeFrame(frame.BmpImage = (Bitmap)(image));
            frame.ThresholdBinary();

            #region Test Saving Binary Image
            Bitmap Test = (Bitmap)image;
            string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Binary.bmp";
            Test.Save(Pw, ImageFormat.Bmp);
            #endregion

            return frame;
        }
        public List<Point> GetConnectedContour(Frame BinaryFrame, Frame NewImage)
        {
            List<Point> Contour = new List<Point>();
            List<Vector2F> CountorVector = new List<Vector2F>();
            int levels = 1;
            CvSeq Sequence = new CvSeq();
            GCHandle Handel;
            IntPtr contours = cvtools.ConvertStructureToPtr(Sequence, out Handel);
            CvMemStorage storage = cvlib.CvCreateMemStorage(0);
            NewImage.IplImageRGB = cvlib.CvCreateImage(new CvSize(BinaryFrame.width, BinaryFrame.height), 8, 1);
            cvlib.CvCvtColor(ref BinaryFrame.IplImageRGB, ref NewImage.IplImageRGB, cvlib.CV_RGB2GRAY);
            cvlib.CvFindContours(ref NewImage.IplImageRGB, ref storage, ref contours, Marshal.SizeOf(typeof(CvContour)), cvlib.CV_RETR_TREE, cvlib.CV_CHAIN_APPROX_SIMPLE, cvlib.CvPoint(0, 0));
            IplImage cnt_img = cvlib.CvCreateImage(cvlib.CvSize(BinaryFrame.width, BinaryFrame.height), 8, 3);
            Sequence = (CvSeq)cvtools.ConvertPtrToStructure(contours, typeof(CvSeq));
            cvlib.CvDrawContours(ref cnt_img, ref Sequence, cvlib.CV_RGB(255, 255, 255), cvlib.CV_RGB(255, 255, 255), levels, 1, cvlib.CV_AA, cvlib.CvPoint(0, 0));
            NewImage.InitializeFrame((Bitmap)cnt_img);
            NewImage.ThresholdBinary();

            #region Convert to CvPoints
            //List<CvPoint> CvContourPoints = new List<CvPoint>();
            //CvPoint point;
            //IntPtr currSeqPtr = contours;
            //for (; currSeqPtr != IntPtr.Zero; currSeqPtr = Sequence.h_next)
            //{
            //    Sequence = (CvSeq)cvtools.ConvertPtrToStructure(currSeqPtr, typeof(CvSeq));
            //    for (int i = 0; i < Sequence.total; i++)
            //    {
            //        IntPtr pointPtr = CvInvoke.cvGetSeqElem(currSeqPtr, i);
            //        point = (CvPoint)cvtools.ConvertPtrToStructure(pointPtr, typeof(CvPoint));
            //        CvContourPoints.Add(point);
            //    }
            //}
            #endregion

            #region Get Points from Image
            int length = NewImage.width, length2 = NewImage.height;

            for (int i = 0; i < length2; i++)
                for (int j = 0; j < length; j++)
                    if (NewImage.byteBluePixels[i, j] != 0)
                        CountorVector.Add(new Vector2F((float)j, (float)i));
            #endregion

            #region Sort Points Clock Wise
            GrahamHull Convex = new GrahamHull();
            Upper = new List<Vector2F>();
            Lower = new List<Vector2F>();
            Convex.Compute(CountorVector, ref Upper, ref Lower);
            //CountorVector = Convex.Compute(CountorVector);
            //int Counter = CountorVector.Count;
            //for (int i = 0; i < Counter; i++)
            //    if (!Contour.Contains(new Point((int)CountorVector[i].X, (int)CountorVector[i].Y)))
            //        Contour.Add(new Point((int)CountorVector[i].X, (int)CountorVector[i].Y));
            #endregion

            #region Test Saving Boundary Image
            //Bitmap Test = NewImage.BmpImage;
            //string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Boundary.bmp";
            //Test.Save(Pw, ImageFormat.Bmp);
            #endregion
            return Contour;
        }
        public void GetUpperAndLowerContour(ref List<Point> UpperList, ref List<Point> LowerList)
        {
            int UpperCount = Upper.Count, LowerCount = Lower.Count;
            for (int i = 0; i < UpperCount; i++)
                UpperList.Add(new Point((int)Upper[i].X, (int)Upper[i].Y));
            for (int i = 0; i < LowerCount; i++)
                LowerList.Add(new Point((int)Lower[i].X, (int)Lower[i].Y));
        }
        #endregion
    }
}
