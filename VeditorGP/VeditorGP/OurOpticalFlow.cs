using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using openCV;
using System.Drawing.Imaging;
using Mapack;
using Sid;

namespace VeditorGP
{
    class OurOpticalFlow
    {
        #region Variables
        public Image<Bgr, Byte> ActualFrame { get; set; }
        public Image<Gray, Byte> ActualGrayFrame { get; set; }
        Class1 OFWAv2;
        public OurOpticalFlow() { }
        #endregion

        #region Get Optical Flow  
        public List<Image<Gray, Single>> OpticalFlowWorker(Frame _CurrentFrame, Image<Gray, Byte> WarpedFrame)
        {
            ActualFrame = _CurrentFrame.EmguRgbImage;
            ActualGrayFrame = ActualFrame.Convert<Gray, Byte>();
            Size winSize = new Size(15, 15);
            Image<Gray, Single> flowx = new Image<Gray, float>(ActualGrayFrame.Size);
            Image<Gray, Single> flowy = new Image<Gray, float>(ActualGrayFrame.Size);
            OpticalFlow.LK(WarpedFrame, ActualGrayFrame, winSize, flowx, flowy);

            int vectorFieldX = (int)Math.Round((double)_CurrentFrame.width / winSize.Width);
            int vectorFieldY = (int)Math.Round((double)_CurrentFrame.height / winSize.Height);

            PointF[][] vectorField = new PointF[vectorFieldX][];
            for (int i = 0; i < vectorFieldX; i++)
            {
                vectorField[i] = new PointF[vectorFieldY];

                for (int j = 0; j < vectorFieldY; j++)
                {
                    Gray velx_gray = flowx[j * winSize.Width, i * winSize.Width];
                    float velx_float = (float)velx_gray.Intensity;
                    Gray vely_gray = flowy[j * winSize.Height, i * winSize.Height];
                    float vely_float = (float)vely_gray.Intensity;
                    vectorField[i][j] = new PointF(velx_float, vely_float);
                }
            }

            List<Image<Gray, Single>> Flow = new List<Image<Gray, Single>>();
            Flow.Add(flowx);
            Flow.Add(flowy);
            return Flow;
        }
        public void ComputeDenseOpticalFlow(Frame _CurrentFrame, Image<Gray, Byte> WarpedFrame)
        {
            // Compute dense optical flow using Horn and Schunk algo
            Image<Gray, float> velx = new Image<Gray, float>(new Size(_CurrentFrame.width , _CurrentFrame.height));
            Image<Gray, float> vely = new Image<Gray, float>(WarpedFrame.Size);
            ActualFrame = _CurrentFrame.EmguRgbImage;
            ActualGrayFrame = ActualFrame.Convert<Gray, Byte>();

            OpticalFlow.HS(ActualGrayFrame, WarpedFrame, true, velx, vely, 0.1d, new MCvTermCriteria(100));

            #region Dense Optical Flow Drawing
            //Size winSize = new Size(10, 10);
            //vectorFieldX = (int)Math.Round((double)faceGrayImage.Width / winSize.Width);
            //vectorFieldY = (int)Math.Round((double)faceGrayImage.Height / winSize.Height);
            //sumVectorFieldX = 0f;
            //sumVectorFieldY = 0f;
            //vectorField = new PointF[vectorFieldX][];
            //for (int i = 0; i < vectorFieldX; i++)
            //{
            //    vectorField[i] = new PointF[vectorFieldY];
            //    for (int j = 0; j < vectorFieldY; j++)
            //    {
            //        Gray velx_gray = velx[j * winSize.Width, i * winSize.Width];
            //        float velx_float = (float)velx_gray.Intensity;
            //        Gray vely_gray = vely[j * winSize.Height, i * winSize.Height];
            //        float vely_float = (float)vely_gray.Intensity;
            //        sumVectorFieldX += velx_float;
            //        sumVectorFieldY += vely_float;
            //        vectorField[i][j] = new PointF(velx_float, vely_float);

            //        Cross2DF cr = new Cross2DF(
            //            new PointF((i * winSize.Width) + trackingArea.X,
            //                       (j * winSize.Height) + trackingArea.Y),
            //                       1, 1);
            //        opticalFlowFrame.Draw(cr, new Bgr(Color.Red), 1);

            //        LineSegment2D ci = new LineSegment2D(
            //            new Point((i * winSize.Width) + trackingArea.X,
            //                      (j * winSize.Height) + trackingArea.Y),
            //            new Point((int)((i * winSize.Width) + trackingArea.X + velx_float),
            //                      (int)((j * winSize.Height) + trackingArea.Y + vely_float)));
            //        opticalFlowFrame.Draw(ci, new Bgr(Color.Yellow), 1);

            //    }
            //}
            #endregion
        }
        #endregion

        #region New Optical Flow
        public void getOpticalFlowWAv2(Frame _CurrentFrame, Image<Gray, Byte> WarpedFrame)
        {
            OFWAv2 = new Class1();
            OFWAv2.Simple(_CurrentFrame, WarpedFrame);
        }
        #endregion
    }
}
