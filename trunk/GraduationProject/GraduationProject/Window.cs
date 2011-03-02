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
    class Window
    {
        public int Center_X, Center_Y, width, height;
        public byte[,] Rpixel, Gpixel, Bpixel;
        public double[,] L, A, B;
        public Window(int _width , int _height, Frame _Frame, CvPoint _CenterPoint) 
        {
            height = _height;
            width = _width;
            if (_width % 2 == 0) width++;
            if (_height % 2 == 0) height++;

            Center_X = _CenterPoint.x;
            Center_Y = _CenterPoint.y;

            Rpixel = new byte[height, width];
            Gpixel = new byte[height, width];
            Bpixel = new byte[height, width];

            L = new double[height, width];
            A = new double[height, width];
            B = new double[height, width];
            int M = (width - 1) / 2, N = (height - 1) / 2;
            for (int i = Center_Y - N , c = 0; i < _Frame.height && i < (Center_Y + N) && c < height; i++, c++)
            {
                if (i < 0) i = 0;
                for (int j = Center_X - M, k = 0; j < _Frame.width && j < (Center_X + M) && k < width; j++, k++)
                {
                    if (j < 0) j = 0;
                    Rpixel[c, k] = _Frame.redPixels[i, j];
                    Gpixel[c, k] = _Frame.greenPixels[i, j];
                    Bpixel[c, k] = _Frame.bluePixels[i, j];

                    L[c, k] = _Frame.L[i, j];
                    A[c, k] = _Frame.A[i, j];
                    B[c, k] = _Frame.B[i, j];
                }
            }
        }
    }
}
