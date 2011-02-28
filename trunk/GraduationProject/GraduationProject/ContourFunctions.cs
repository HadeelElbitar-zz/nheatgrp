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
        public ContourFunctions() { }
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
            FrameFunctions FFn = new FrameFunctions();
            FFn.CopyFrameData(cnt_img, ref NewImage);
            return NewImage;
        }
    }
}
