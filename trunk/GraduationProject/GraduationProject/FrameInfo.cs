using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GraduationProject
{
    public class FrameInfo
    {
        public int width;
        public int height;
        public PictureBox FrameBox;
        public byte[,] redPixels;
        public byte[,] greenPixels;
        public byte[,] bluePixels;
        public FrameInfo() { }
        public FrameInfo(int _width, int _height, PictureBox _frameBox, byte[,] _redPixels, byte[,] _greenPixels, byte[,] _bluePixels)
        {
            width = _width;
            height = _height;
            FrameBox = _frameBox;
            redPixels = _redPixels;
            greenPixels = _greenPixels;
            bluePixels = _bluePixels;
        }
        public FrameInfo(FrameInfo pic)
        {
            width = pic.width;
            height = pic.height;
            FrameBox = pic.FrameBox;
            redPixels = new byte[height, width];
            greenPixels = new byte[height, width];
            bluePixels = new byte[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    redPixels[i, j] = pic.redPixels[i, j];
                    greenPixels[i, j] = pic.greenPixels[i, j];
                    bluePixels[i, j] = pic.bluePixels[i, j];
                }
            }
        }
    }
}
