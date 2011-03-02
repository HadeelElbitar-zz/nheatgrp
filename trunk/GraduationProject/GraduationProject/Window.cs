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
        }
    }
}
