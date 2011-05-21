using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace EMTester
{
    public partial class Form1 : Form
    {
        #region Variables and Constructor
        public Form1()
        {
            InitializeComponent();
        }
        Bitmap BmpImage;
        byte[,] redPixels, greenPixels, bluePixels;
        int width, height; 
        #endregion

        #region Open Image
        private void openImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] PicturePath = new string[20];
            OpenFileDialog Picture = new OpenFileDialog();
            Picture.Filter = "All Files (*.*)|*.*";
            Picture.Multiselect = true;
            if (Picture.ShowDialog() == DialogResult.OK)
                PicturePath = Picture.FileNames;
            int count = PicturePath.Count();
            for (int k = 0; k < count; k++)
            {
                string PictureName = PicturePath[k].Substring(PicturePath[k].LastIndexOf('\\') + 1);
                int offset = PictureName.LastIndexOf('.') + 1;
                string type = PictureName.Substring(offset, PictureName.Length - offset);
                BmpImage = new Bitmap(PicturePath[k]);
            }
            width = BmpImage.Width;
            height = BmpImage.Height;
            redPixels = new byte[height, width];
            greenPixels = new byte[height, width];
            bluePixels = new byte[height, width];
            BitmapData bmpData = BmpImage.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, BmpImage.PixelFormat);
            FillFrameRGB(bmpData);
            OpenPicBox.Image = BmpImage;
        }
        void FillFrameRGB(BitmapData bmpData)
        {
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                if (BmpImage.PixelFormat == PixelFormat.Format64bppArgb || BmpImage.PixelFormat == PixelFormat.Format64bppPArgb)
                {
                    int space = bmpData.Stride - width * 8;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
                            p += 8;
                        }
                        p += space;
                    }
                }
                if (BmpImage.PixelFormat == PixelFormat.Format32bppArgb || BmpImage.PixelFormat == PixelFormat.Format32bppRgb)
                {
                    int space = bmpData.Stride - width * 4;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
                            p += 4;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    int space = bmpData.Stride - width * 3;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
                            p += 3;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format16bppRgb555)
                {
                    int space = bmpData.Stride - width * 2;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bluePixels[i, j] = p[0];
                            greenPixels[i, j] = p[1];
                            redPixels[i, j] = p[2];
                            p += 2;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    int space = bmpData.Stride - width;
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = p[0];
                            p++;
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format4bppIndexed)
                {
                    int space = bmpData.Stride - ((width / 2) + (width % 2));
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            int e = ((j + 1) % 2 == 0) ? p[0] >> 4 : p[0] & 0x0F;
                            bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = p[e];
                            p += ((j + 1) % 2);
                        }
                        p += space;
                    }
                }
                else if (BmpImage.PixelFormat == PixelFormat.Format1bppIndexed)
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            byte* ptr = (byte*)bmpData.Scan0 + (i * bmpData.Stride) + (j / 8);
                            byte b = *ptr;
                            byte mask = Convert.ToByte(0x80 >> (j % 8));
                            if ((b & mask) != 0)
                                bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = 255;
                            else
                                bluePixels[i, j] = greenPixels[i, j] = redPixels[i, j] = 0;
                        }
                    }
                }
            }
            BmpImage.UnlockBits(bmpData);
        } 
        #endregion

        private void EmBTN_Click(object sender, EventArgs e)
        {
            EmClass Em = new EmClass(redPixels, greenPixels, bluePixels, width, height);
        }
    }
}
