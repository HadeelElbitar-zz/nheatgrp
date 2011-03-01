using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

// add this usings 
using Emgu.CV;
using Emgu.CV.Structure;
using openCV;
using Emgu.CV.UI;
using System.Runtime.InteropServices;



namespace TesTOpenCV
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
        }

        private unsafe void button1_Click(object sender, EventArgs e)
        {
            int levels = 1;
            CvSeq p = new CvSeq();
            GCHandle h;
            IntPtr contours = cvtools.ConvertStructureToPtr(p, out h);

            CvMemStorage storage = cvlib.CvCreateMemStorage(0);

            IplImage img;
            img = cvlib.CvLoadImage("D:\\GP\\capture.bmp", cvlib.CV_LOAD_IMAGE_GRAYSCALE);
            cvlib.CvNamedWindow("image", cvlib.CV_WINDOW_AUTOSIZE);
            cvlib.CvShowImage("image", ref img); ;

            cvlib.CvFindContours(ref img, ref storage, ref contours, Marshal.SizeOf(typeof(CvContour)), cvlib.CV_RETR_TREE, cvlib.CV_CHAIN_APPROX_SIMPLE, cvlib.CvPoint(0, 0));

            int x_1 = cvlib.CvNamedWindow("contours", cvlib.CV_WINDOW_AUTOSIZE);

            IplImage cnt_img = cvlib.CvCreateImage(cvlib.CvSize(500, 500), 8, 3);

            IntPtr currSeqPtr = contours;
            p = (CvSeq)cvtools.ConvertPtrToStructure(currSeqPtr, typeof(CvSeq));
            
            cvlib.CvDrawContours(ref cnt_img, ref p, cvlib.CV_RGB(255, 0, 0), cvlib.CV_RGB(0, 255, 0), levels, 3, cvlib.CV_AA, cvlib.CvPoint(0, 0));

            //Emgu.CV.Structure.MCvSeqReader reader = new MCvSeqReader();
            CvPoint point;
            for (; currSeqPtr != IntPtr.Zero; currSeqPtr = p.h_next)
            {
                p = (CvSeq)cvtools.ConvertPtrToStructure(currSeqPtr, typeof(CvSeq));
                //CvInvoke.cvStartReadSeq(contours,ref reader, false);
                for (int i = 0; i < p.total; i++)
                {
                    IntPtr pointPtr = CvInvoke.cvGetSeqElem(currSeqPtr, i);//CvInvoke.CV_READ_SEQ_ELEM<CvPoint>(ref reader);
                    point = (CvPoint)cvtools.ConvertPtrToStructure(pointPtr, typeof(CvPoint));
                   //CvInvoke.CV_NEXT_SEQ_ELEM(sizeof(CvPoint), ref reader);
                }
            }
            //CvInvoke.cvCvtSeqToArray(
           // p.
            

            cvlib.CvShowImage("contours", ref cnt_img);
        }
    }
}
