using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using openCV;
using Emgu.CV.UI;
using System.Runtime.InteropServices;

namespace GraduationProject
{
    class FrameFunctions
    {
        public FrameFunctions() { }
        public void DisplayFrame(Frame pic, PictureBox Box)
        {
            pic.FrameBox = Box;
            pic.FrameBox.Size = new System.Drawing.Size(pic.width, pic.height);
            pic.FrameBox.Image = pic.BmpImage;
        }
        public void CopyFrameData(IplImage Source, ref Frame Dest)
        {
            int height = Dest.height;
            int width = Dest.width;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    CvScalar vals = cvlib.CvGet2D(ref Source, i, j);
                    Dest.bluePixels[i, j] = (byte)vals.val1;
                    Dest.greenPixels[i, j] = (byte)vals.val2;
                    Dest.redPixels[i, j] = (byte)vals.val3;
                }
            }
        }
    }
}
