using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using openCV;
using System.Drawing.Imaging;

namespace VeditorGP
{
    class OurOpticalFlow
    {
        #region Variables
        public Image<Bgr, Byte> ActualFrame { get; set; }
        public Image<Gray, Byte> ActualGrayFrame { get; set; }
        #endregion

        #region Get Optical Flow
        public OurOpticalFlow() { }
        public List<Image<Gray, Single>> OpticalFlowWorker(Frame _CurrentFrame, Image<Gray, Byte> WarpedFrame)
        {
            ActualFrame = _CurrentFrame.EmguRgbImage;
            ActualGrayFrame = ActualFrame.Convert<Gray, Byte>();
            Image<Gray, Single> flowx = new Image<Gray, float>(ActualGrayFrame.Size);
            Image<Gray, Single> flowy = new Image<Gray, float>(ActualGrayFrame.Size);
            OpticalFlow.LK(ActualGrayFrame, WarpedFrame, new Size(15, 15), flowx, flowy);
            List<Image<Gray, Single>> Flow = new List<Image<Gray, Single>>();
            Flow.Add(flowx);
            Flow.Add(flowy);
            return Flow;
        } 
        #endregion
    }
}
