using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using openCV;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.Structure;
using System.Drawing.Imaging;

namespace VeditorGP
{
    class ContourFunctions
    {
        public ContourFunctions() { }

        #region Mask Frame and Contour
        public Frame GetBlackAndWhiteContour(CvPoint[] pts, Bitmap BmpImage)
        {
            int width = BmpImage.Width;
            int height = BmpImage.Height;
            IplImage image = cvlib.ToIplImage((Bitmap)BmpImage, true);
            cvlib.CvSetZero(ref image);
            cvlib.CvFillConvexPoly(ref image, ref pts[0], pts.Count(), cvlib.CV_RGB(255, 255, 255), cvlib.CV_AA, 0);
            Frame frame = new Frame();
            frame.InitializeFrame(frame.BmpImage = (Bitmap)(image));
            return frame;
        }
        public List<Point> GetConnectedContour(Frame BinaryFrame)
        {
            List<Point> Contour = new List<Point>();
            #region Boundary Extraction
            int height = BinaryFrame.height, width = BinaryFrame.width, widthSE = 3, heightSE = 3;
            int[,] StructerElement = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            byte[,] NewPicR = new byte[height, width];
            byte[,] NewPicG = new byte[height, width];
            byte[,] NewPicB = new byte[height, width];
            bool flag = false;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    flag = false;
                    for (int c = 0; c < heightSE; c++)
                    {
                        for (int k = 0; k < widthSE; k++)
                        {
                            if (StructerElement[c, k] == 1 && BinaryFrame.byteRedPixels[i + c, j + k] == 0)
                            {
                                NewPicR[i, j] = 0;
                                NewPicG[i, j] = 0;
                                NewPicB[i, j] = 0;
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                            break;
                        NewPicR[i, j] = 255;
                        NewPicG[i, j] = 255;
                        NewPicB[i, j] = 255;
                    }
                }
            }
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    NewPicR[i, j] = (byte)(BinaryFrame.byteRedPixels[i, j] - NewPicR[i, j]);
                    NewPicG[i, j] = (byte)(BinaryFrame.byteGreenPixels[i, j] - NewPicG[i, j]);
                    NewPicB[i, j] = (byte)(BinaryFrame.byteBluePixels[i, j] - NewPicB[i, j]);
                }
            #endregion
            #region Fill Contour List
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    if (NewPicB[i, j] == 255)
                        Contour.Add(new Point(j, i));
            #endregion
            #region Test Saving Boundary Image
            //Bitmap NewImage = new Bitmap(width, height);
            //BitmapData bmpData = NewImage.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, NewImage.PixelFormat);
            //unsafe
            //{
            //    byte* p = (byte*)bmpData.Scan0;
            //    int space = bmpData.Stride - width * 3;
            //    for (int i = 0; i < height; i++)
            //    {
            //        for (int j = 0; j < width; j++)
            //        {
            //            p[0] = NewPicB[i, j];
            //            p[1] = NewPicG[i, j];
            //            p[2] = NewPicR[i, j];
            //            p += 3;
            //        }
            //        p += space;
            //    }
            //}
            //NewImage.UnlockBits(bmpData);
            //string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Boundary.bmp";
            //NewImage.Save(Pw, ImageFormat.Bmp); 
            #endregion
            return Contour;
        }
        #endregion
    }
}
