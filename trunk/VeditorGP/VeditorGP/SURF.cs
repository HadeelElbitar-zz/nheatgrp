using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.UI;

namespace VeditorGP
{
    class SURF
    {
        public SURF() { }
        public SURFTracker.MatchedSURFFeature[] GetSIFTpoints(Frame CurrentFrame, Frame NextFrame)
        {
            MCvSURFParams surfParam = new MCvSURFParams(500, false);
            Image<Gray, Byte> modelImage = CurrentFrame.EmguRgbImage.Convert<Gray, Byte>();
            SURFFeature[] modelFeatures = modelImage.ExtractSURF(ref surfParam);
            Image<Gray, Byte> observedImage = NextFrame.EmguRgbImage.Convert<Gray, Byte>();
            SURFFeature[] imageFeatures = observedImage.ExtractSURF(ref surfParam);
            SURFTracker tracker = new SURFTracker(modelFeatures);
            SURFTracker.MatchedSURFFeature[] matchedFeatures = tracker.MatchFeature(imageFeatures, 2, 20);
            matchedFeatures = SURFTracker.VoteForUniqueness(matchedFeatures, 0.8);
            matchedFeatures = SURFTracker.VoteForSizeAndOrientation(matchedFeatures, 1.5, 20);
            HomographyMatrix homography = SURFTracker.GetHomographyMatrixFromMatchedFeatures(matchedFeatures); // mesh fhma de bt3t eah!
            #region draw lines between the matched features

            //int goodMatchCount = 0, x;
            //foreach (SURFTracker.MatchedSURFFeature ms in matchedFeatures)
            //    if (ms.Distances[0] < 0.5)
            //        goodMatchCount++;
            //    else
            //        x = 0;

            //Image<Gray, Byte> res = modelImage.ConcateVertical(observedImage);
            //foreach (SURFTracker.MatchedSURFFeature matchedFeature in matchedFeatures)
            //{
            //    PointF p = matchedFeature.ObservedFeature.Point.pt;
            //    p.Y += modelImage.Height;
            //    res.Draw(new LineSegment2DF(matchedFeature.ModelFeatures[0].Point.pt, p), new Gray(0), 1);
            //}

            #endregion
            #region draw the project region on the image
            //  if (homography != null)
            //  {  //draw a rectangle along the projected model
            //      Rectangle rect = modelImage.ROI;
            //      PointF[] pts = new PointF[] { 
            //         new PointF(rect.Left, rect.Bottom),
            //         new PointF(rect.Right, rect.Bottom),
            //         new PointF(rect.Right, rect.Top),
            //         new PointF(rect.Left, rect.Top)};
            //      homography.ProjectPoints(pts);
            //      for (int i = 0; i < pts.Length; i++)
            //          pts[i].Y += modelImage.Height;
            //      res.DrawPolyline(Array.ConvertAll<PointF, Point>(pts, Point.Round), true, new Gray(255.0), 5);
            //  }
            #endregion   Bitmap NewPic = null;
            //  Bitmap NewPic = res.ToBitmap(res.Width, res.Height);
            return matchedFeatures;
        }
    }
}
