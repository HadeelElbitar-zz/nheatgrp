using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using openCV;
using System.Drawing.Imaging;
using Mapack;
using Sid;
//using VeditorGP;

namespace VeditorGP
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
        static int Counter = 0;
        static void Simple()
        {
            //string dir = "C:\\Users\\DyDy\\Pictures\\testCase"; // the directory where images are stored
            //int N = 2;
            //string[] filenames = new string[N];
            //WAdetector WA = new WAdetector();
            //int h = 216;
            //int w = 373;
            //int e = (int)Math.Round(0.5 * (Math.Log(h, 2) + Math.Log(w, 2)));
            //int n = e - 5;
            //if (n < 0) n = 0;
            //if (n > 3) n = 3;
            //int[] levels_to_compute = new int[] { n };
            //float[, ,] f = WA.WAComputeOpticalFlow2(80, 2, 16, 10, levels_to_compute, filenames);
            //float[, ,] quiver_data = new float[f.GetLength(0), f.GetLength(1), 2];	// for the quiver plot
            //for (int y = 0; y < quiver_data.GetLength(0); y++)
            //    for (int x = 0; x < quiver_data.GetLength(1); x++)
            //    {
            //        quiver_data[y, x, 0] = f[y, x, 0] * f[y, x, 2]; // scale vx by weight w
            //        quiver_data[y, x, 1] = f[y, x, 1] * f[y, x, 2]; // scale vy by weight w
            //    }
            //Bitmap bmp = WAdetector.Quiver(quiver_data);	// creates a bitmap to visualize the data in quiver_data
            //int date = DateTime.Now.Year * 1000 + DateTime.Now.Month * 100 + DateTime.Now.Day;	// get date in yyyymmdd format
            //int time = DateTime.Now.Hour * 1000 + DateTime.Now.Minute * 100 + DateTime.Now.Second; // get time in hhmmss format
            //string filename = dir + "\\simple_flow-" + date + "-" + time + ".bmp";
            //bmp.Save(filename); // save the quiver plot
        }

        public void Simple(Frame _CurrentFrame, Image<Gray, Byte> WarpedFrame)
        {
          // string dir = "C:\\Users\\DyDy\\Pictures\\testCase"; // the directory where images are stored
            int N = 2;
            string[] filenames = new string[N];
            WAdetector WA = new WAdetector();
            int h = _CurrentFrame.height;
            int w = _CurrentFrame.width;
            int e = (int)Math.Round(0.5 * (Math.Log(h, 2) + Math.Log(w, 2)));
            int n = e - 5;
            if (n < 0) n = 0;
            if (n > 3) n = 3;
            int[] levels_to_compute = new int[] { n };
            float[, ,] f = WA.WAComputeOpticalFlow2(80, 2, 16, 10, levels_to_compute, _CurrentFrame.BmpImage, WarpedFrame.Bitmap);
            float[, ,] quiver_data = new float[f.GetLength(0), f.GetLength(1), 2];	// for the quiver plot
            //for (int y = 0; y < quiver_data.GetLength(0); y++)
            //    for (int x = 0; x < quiver_data.GetLength(1); x++)
            //    {
            //        quiver_data[y, x, 0] = f[y, x, 0] * f[y, x, 2]; // scale vx by weight w
            //        quiver_data[y, x, 1] = f[y, x, 1] * f[y, x, 2]; // scale vy by weight w
            //    }
            float[, ,] Result = WAdetector.Quiver(f);
            //Bitmap bmp = WAdetector.Quiver(quiver_data);	// creates a bitmap to visualize the data in quiver_data
            //string Pw;
            //Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Optical Flow From " + Counter + " to " + (Counter+1) +".bmp";
            //Counter++;
            //bmp.Save(Pw); // save the quiver plot
        }
	}
}
