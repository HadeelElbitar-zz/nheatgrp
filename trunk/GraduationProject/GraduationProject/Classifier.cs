using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics.Distributions.Multivariate;

namespace GraduationProject
{
    class Classifier
    {
        public GaussianMixtureModel GMM;

        int m;
        double[][] mixture;


        public Classifier() { }
        public void TrainClassifier(Window _Window)
        {
            GMM = new GaussianMixtureModel(3);
            int NewSize = _Window.WinFrame.height * _Window.WinFrame.width;

            double[][] B = new double[NewSize][];
            int h = 0;
            int w = 0; 
            for (int i = 0; i < NewSize; i++)
            {
                if (w > _Window.WinFrame.width-1)
                {
                    h++;
                    w = 0; 
                }
                if (h > _Window.WinFrame.height-1)
                    break;
                B[i] = new double[3]; 
                B[i][0] = (double)_Window.WinFrame.redPixels[h, w];
                B[i][1] = (double)_Window.WinFrame.greenPixels[h, w];
                B[i][2] = (double)_Window.WinFrame.bluePixels[h, w];

                w++; 
            }
            GMM.Compute(B); 


            double[][][] A = new double[_Window.WinFrame.height][][];
            for (int i = 0; i < _Window.WinFrame.height; i++)
            {
                A[i] = new double[_Window.WinFrame.width][];
                for (int j = 0; j < _Window.WinFrame.width; j++)
                {
                    A[i][j] = new double[3];
                    {
                        A[i][j][0] = (double)_Window.WinFrame.redPixels[i, j];
                        A[i][j][1] = (double)_Window.WinFrame.greenPixels[i, j];
                        A[i][j][2] = (double)_Window.WinFrame.bluePixels[i, j];
                    }
                }
            }
         //   double[][] B = Matrix.Combine(A);
            GMM.Compute(B);

            //double[][][] A = new double[3][][];
            //A[0] = new double[_Window.WinFrame.height][];
            //A[1] = new double[_Window.WinFrame.height][];
            //A[2] = new double[_Window.WinFrame.height][];
            ////int i = -1;
            //for (int j = 0; j < _Window.WinFrame.height; j++)
            //{
            //    A[0][j] = new double[_Window.WinFrame.width];
            //    A[1][j] = new double[_Window.WinFrame.width];
            //    A[2][j] = new double[_Window.WinFrame.width];
            //    for (int k = 0; k < _Window.WinFrame.width; k++)
            //    {
            //        A[0][j][k] = (double)_Window.WinFrame.redPixels[j, k];
            //        A[1][j][k] = (double)_Window.WinFrame.greenPixels[j, k];
            //        A[2][j][k] = (double)_Window.WinFrame.bluePixels[j, k];
            //    }
            //}
            //double[][] B = Matrix.Combine(A);

            //GMM.Compute(B);
        //    GMM.Compute(B);



            m = 3;//(int)numClusters.Value;

            // Generate data with n Gaussian distributions
            double[][][] data = new double[m][][];

            for (int i = 0; i < m; i++)
            {
                // Create random centroid to place the Gaussian distribution
                var mean = Matrix.Random(2, -6.0, +6.0);

                // Create random covariance matrix for the distribution
                double[,] covariance;
                do
                {
                    covariance = Matrix.Random(2, true, 0.0, 3.0);
                }
                while (!covariance.IsPositiveDefinite());


                // Create the Gaussian distribution
                var gaussian = new NormalDistribution(mean, covariance);

                int samples = Accord.Math.Tools.Random.Next(150, 250);
                data[i] = gaussian.Generate(samples);
            }

            // Join the generated data
            mixture = Matrix.Combine(data);

            // Create a new Gaussian Mixture Model
            GaussianMixtureModel gmm = new GaussianMixtureModel(m);

            // Compute the model
            gmm.Compute(mixture);
        }

        private void btnGenerateRandom_Click(object sender, EventArgs e)
        {
           
        }
    }
}
