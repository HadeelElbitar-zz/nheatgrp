using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace VeditorGP
{
    class OurOpticalFlow
    {
        #region Variables

        public Image<Bgr, Byte> ActualFrame { get; set; }
        public Image<Bgr, Byte> NextFrame { get; set; }
        public Image<Bgr, Byte> OpticalFlowFrame { get; set; }
        public Image<Gray, Byte> ActualGrayFrame { get; set; }
        public Image<Gray, Byte> NextGrayFrame { get; set; }
        public PointF[][] ActualFeature;
        public PointF[] NextFeature;
        public Byte[] Status;
        public float[] TrackError;
        #endregion
        public OurOpticalFlow() { }
        public PointF[] OpticalFlowWorker(Frame _CurrentFrame, Frame _NextFrame, SURFTracker.MatchedSURFFeature[] OpticalFlowWorker)
        {
            ActualFrame = _CurrentFrame.EmguRgbImage;
            ActualGrayFrame = ActualFrame.Convert<Gray, Byte>();
            NextFrame = _NextFrame.EmguRgbImage;
            NextGrayFrame = NextFrame.Convert<Gray, Byte>();
            ActualFeature = new PointF[1][];
            ActualFeature[0] = new PointF[OpticalFlowWorker.Length + 1];
            for (int i = 0; i < OpticalFlowWorker.Length; i++)
                ActualFeature[0][i] = OpticalFlowWorker[i].ObservedFeature.Point.pt;
            OpticalFlow.PyrLK(ActualGrayFrame, NextGrayFrame, ActualFeature[0], new System.Drawing.Size(10, 10), 3, new MCvTermCriteria(20, 0.03d), out NextFeature, out Status, out TrackError);
            OpticalFlowFrame = new Image<Bgr, Byte>(ActualFrame.Width, ActualFrame.Height);
            OpticalFlowFrame = NextFrame.Copy();
            #region VisualizeFlow
            for (int i = 0; i < ActualFeature[0].Length; i++)
                DrawFlowVectors(i);
            #endregion
            return NextFeature;
        }
        private void DrawFlowVectors(int i)
        {

            System.Drawing.Point p = new Point();
            System.Drawing.Point q = new Point();

            p.X = (int)ActualFeature[0][i].X;
            p.Y = (int)ActualFeature[0][i].Y;
            q.X = (int)NextFeature[i].X;
            q.Y = (int)NextFeature[i].Y;

            double angle;
            angle = Math.Atan2((double)p.Y - q.Y, (double)p.X - q.X);

            LineSegment2D line = new LineSegment2D(p, q);
            OpticalFlowFrame.Draw(line, new Bgr(255, 0, 0), 1);

            p.X = (int)(q.X + 6 * Math.Cos(angle + Math.PI / 4));
            p.Y = (int)(q.Y + 6 * Math.Sin(angle + Math.PI / 4));
            OpticalFlowFrame.Draw(new LineSegment2D(p, q), new Bgr(255, 0, 0), 1);
            p.X = (int)(q.X + 6 * Math.Cos(angle - Math.PI / 4));
            p.Y = (int)(q.Y + 6 * Math.Sin(angle - Math.PI / 4));
            OpticalFlowFrame.Draw(new LineSegment2D(p, q), new Bgr(255, 0, 0), 1);
        }
    }
}
