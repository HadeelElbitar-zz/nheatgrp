using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using openCV;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.ML;
namespace GraduationProject
{
    class Classifier
    {
        MCvBGStatModel yy;
        MCvFGDStatModelParams ii;

        //Matrix<int> em = new Matrix<int>(1, 1);
        //EMParams em = new EMParams();
        public Classifier() { }
        public void TrainClassifier(Window _Window)
        {
            //int Classes = 2;
            #region Get Random Centroids
            //double[] centroids = new double[Classes];
            //Random rand = new Random();
            //for (int k = 0; k < Classes; k++)
            //{
            //    int i = rand.Next(0, _Window.WinFrame.height);
            //    int j = rand.Next(0, _Window.WinFrame.width);
            //    centroids[k] = (_Window.WinFrame.redPixels[i, j] + _Window.WinFrame.greenPixels[i, j] + _Window.WinFrame.bluePixels[i, j]) / 3;
            //}
            #endregion

            #region Centroids Convergence

            //bool change = true;
            //while (change)
            //{
            //    double[] CentroidAssignedValues = new double[Classes];
            //    double[] CentroidAssignedFrequencies = new double[Classes];

            //    for (int i = 0; i < _Window.WinFrame.height; i++)
            //    {
            //        for (int j = 0; j < _Window.WinFrame.width; j++)
            //        {
            //            double Min = double.MaxValue;
            //            for (int r = 0; r < Classes; r++)
            //            {
            //                double Difference = Math.Abs(((_Window.WinFrame.redPixels[i, j] + _Window.WinFrame.greenPixels[i, j] + _Window.WinFrame.bluePixels[i, j]) / 3) - centroids[r]);
            //                if (Difference < Min)
            //                {
            //                    Min = Difference;
            //                    CentroidAssignedFrequencies[r]++;
            //                    CentroidAssignedValues[r] += ((_Window.WinFrame.redPixels[i, j] + _Window.WinFrame.greenPixels[i, j] + _Window.WinFrame.bluePixels[i, j]) / 3);
            //                    _Window.AfterCalcPointClass[i, j] = r + 1;
            //                }
            //            }
            //        }
            //    }
            //    change = false;
            //    for (int c = 0; c < Classes; c++)
            //    {
            //        double temp = CentroidAssignedValues[c] / CentroidAssignedFrequencies[c];
            //        if (temp != centroids[c] && temp != double.NaN)
            //        {
            //            centroids[c] = temp;
            //            change = true;
            //        }
            //    }
            //}
            #endregion
        }
    }
}
