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


namespace TesTOpenCV
{
    public partial class Form1 : Form
    {
        Capture temp;
      //  Emgu.CV.UI.ImageBox ImageBox = new ImageBox();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            temp = new Capture("D:\\tree.avi");
         //   ImageBox.Location.X = 12;
          //  ImageBox.Location.Y = 12; 
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           // try
            {
                {
                    using (Image<Bgr, byte> pic = temp.QueryFrame())
                    {
                        imageBox1.Image = pic;
                    }
                }
            }
         //   catch
            {
           //     MessageBox.Show("Filtype not valid.", "Error while loading a file.",
             //       MessageBoxButtons.OK, MessageBoxIcon.Error);
              //  return;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //IplImage snapshotGrayScale = cvlib.CvCreateImage(cvlib.CvGetSize(ref snapshot), snapshot.depth, 1);
            //IplImage snapshotDerivative = cvlib.CvCreateImage(cvlib.CvGetSize(ref snapshot), (int)cvlib.IPL_DEPTH_32F, 1);

            //cvlib.CvConvertImage(ref snapshot, ref snapshotGrayScale, 0);   // Met l'image en grayscale
            //cvlib.CvLaplace(ref snapshotGrayScale, ref snapshotDerivative, 5);


            //cvlib.CvNamedWindow("Originale", cvlib.CV_WINDOW_AUTOSIZE);
            //cvlib.CvNamedWindow("Grayscale", cvlib.CV_WINDOW_AUTOSIZE);
            //cvlib.CvNamedWindow("Laplace", cvlib.CV_WINDOW_AUTOSIZE);

            //cvlib.CvShowImage("Originale", ref snapshot);
            //cvlib.CvShowImage("Grayscale", ref snapshotGrayScale);
            //cvlib.CvShowImage("Laplace", ref snapshotDerivative);
        }
    }
}
