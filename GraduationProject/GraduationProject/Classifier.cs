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
using Emgu.CV.ML.Structure;

namespace GraduationProject
{
    class Classifier
    {
        //MCvBGStatModel yy;
        //MCvFGDStatModelParams ii;
        double[] Kmean1, Kmean2;
        double[,] KmeanCovarinace1, KmeanCovarinace2;
        //Matrix<int> em = new Matrix<int>(1, 1);
        //EMParams em = new EMParams();
        public Classifier() { }
        public void EM(Window _Window)
        {
            Random r = new Random(DateTime.Now.Millisecond);
            int N = _Window.WinFrame.height * _Window.WinFrame.width;//3adad el data set
            int D = 3;

            EM Em = new EM();
            Matrix<int> labels = new Matrix<int>(N, 1);
            Matrix<float> featuresM = new Matrix<float>(N, D);
            int PatternCount = 0;
            for (int i = 0; i < _Window.WinFrame.height; i++)
                for (int j = 0; j < _Window.WinFrame.width; j++)
                {
                    featuresM[PatternCount, 0] = _Window.WinFrame.redPixels[i, j];
                    featuresM[PatternCount, 1] = _Window.WinFrame.greenPixels[i, j];
                    featuresM[PatternCount, 2] = _Window.WinFrame.bluePixels[i, j];
                    PatternCount++;
                }

            EMParams pars = new EMParams();
            Matrix<double>[] Covariances = new Matrix<double>[2];
            Covariances[0] = new Matrix<double>(KmeanCovarinace1);
            Covariances[1] = new Matrix<double>(KmeanCovarinace2);
            pars.Covs = Covariances;
            double[,] OurKmeans = new double[2, 3];
            for (int i = 0; i < 3; i++)
            {
                OurKmeans[0, i] = Kmean1[i];
                OurKmeans[1, i] = Kmean2[i];
            }
            pars.Means = new Matrix<double>(OurKmeans);
            //pars.CovMatType = Emgu.CV.ML.MlEnum.EM_COVARIAN_MATRIX_TYPE.COV_MAT_DIAGONAL;
            pars.Nclusters = 2;
            pars.StartStep = Emgu.CV.ML.MlEnum.EM_INIT_STEP_TYPE.START_AUTO_STEP;
            pars.TermCrit = new MCvTermCriteria(100, 1.0e-6);

            Em.Train(featuresM, null, pars, labels);
            IntPtr Means = Em.Means.MCvMat.data;
            double x;
            unsafe
            {
                double* PTR = (double*)Means;
                x = *PTR;
            }
            Matrix<double>[] Covariance = Em.GetCovariances();
        }
        public void Kmean(Window _Window)
        {
            int Classes = 2;

            #region Get Random Centroids
            List<double[]> centroids = new List<double[]>();
            
            Random rand = new Random();
            
            for (int k = 0; k < Classes; k++)
            {
                int i = rand.Next(0, _Window.WinFrame.height);
                int j = rand.Next(0, _Window.WinFrame.width);
                double[] temp = new double[3];
                temp[0] = _Window.WinFrame.redPixels[i, j];
                temp[1] = _Window.WinFrame.greenPixels[i, j];
                temp[2] = _Window.WinFrame.bluePixels[i, j];
                centroids.Add(temp);
            }
            while (centroids[0][0] == centroids[1][0] && centroids[0][1] == centroids[1][1] && centroids[0][2] == centroids[1][2])
            {
                centroids.Clear();
                for (int k = 0; k < Classes; k++)
                {
                    int i = rand.Next(0, _Window.WinFrame.height);
                    int j = rand.Next(0, _Window.WinFrame.width);
                    double[] temp = new double[3];
                    temp[0] = _Window.WinFrame.redPixels[i, j];
                    temp[1] = _Window.WinFrame.greenPixels[i, j];
                    temp[2] = _Window.WinFrame.bluePixels[i, j];
                    centroids.Add(temp);
                }
            }
            #endregion

            #region Centroids Convergence
            bool change = true;
            while (change)
            {
                List<double[]> Mean1Vectors = new List<double[]>();
                List<double[]> Mean2Vectors = new List<double[]>();
                for (int i = 0; i < _Window.WinFrame.height; i++)
                {
                    for (int j = 0; j < _Window.WinFrame.width; j++)
                    {
                        double[] DataVector = new double[3];
                        DataVector[0] = _Window.WinFrame.redPixels[i, j];
                        DataVector[1] = _Window.WinFrame.greenPixels[i, j];
                        DataVector[2] = _Window.WinFrame.bluePixels[i, j];
                        double Distance1 = Distance(DataVector, centroids[0]);
                        double Distance2 = Distance(DataVector, centroids[1]);
                        if (Distance1 < Distance2)
                        {
                            Mean1Vectors.Add(DataVector);
                            _Window.AfterCalcPointClass[i, j] = 1;
                        }
                        else
                        {
                            Mean2Vectors.Add(DataVector);
                            _Window.AfterCalcPointClass[i, j] = 2;
                        }
                    }
                }
                change = false;
                double[] NewMean1 = FindNewMean(Mean1Vectors);
                double[] NewMean2 = FindNewMean(Mean2Vectors);
                double TempDiff1 = Distance(NewMean1, centroids[0]);
                double TempDiff2 = Distance(NewMean2, centroids[1]);
                if (TempDiff1 > 0.001)
                {
                    centroids[0] = NewMean1;
                    change = true;
                }
                if (TempDiff2 > 0.001)
                {
                    centroids[1] = NewMean2;
                    change = true;
                }
            }
            #endregion

            Kmean1 = centroids[0];
            Kmean2 = centroids[1];
            KmeanCovarinace1 = GetCovariance(Kmean1, _Window);
            KmeanCovarinace2 = GetCovariance(Kmean2, _Window);
            EM(_Window);
        }
        double Distance(double[] A, double[] B)
        {
            double Result = 0.0;
            Result = Math.Sqrt(Math.Pow(A[0] - B[0], 2) + Math.Pow(A[1] - B[1], 2) + Math.Pow(A[2] - B[2], 2));
            return Result;
        }
        double[] FindNewMean(List<double[]> List)
        {
            double[] Result = new double[3];
            
            foreach (double[] item in List)
            {
                Result[0] += item[0];
                Result[1] += item[1];
                Result[2] += item[2];
            }
            Result[0] /= List.Count;
            Result[1] /= List.Count;
            Result[2] /= List.Count;
            return Result;
        }
        double[,] GetCovariance(double[] _MeanMatrix, Window _Win)
        {
            double[] MeanMatrix = _MeanMatrix;
            double[,] CovarianceMatrix = new double[3, 3];
            Window Win = _Win;
            double ReadMean = 0, GreenMean = 0, BlueMean = 0;
            for (int i = 0; i < Win.WinFrame.height; i++)
            {
                for (int j = 0; j < Win.WinFrame.width; j++)
                {
                    ReadMean += MeanMatrix[0] - Win.WinFrame.redPixels[i, j];
                    GreenMean += MeanMatrix[1] - Win.WinFrame.redPixels[i, j];
                    BlueMean += MeanMatrix[2] - Win.WinFrame.redPixels[i, j];
                }
            }
            CovarianceMatrix[0, 0] = ReadMean / (Win.WinFrame.width * Win.WinFrame.height);
            CovarianceMatrix[1, 1] = GreenMean / (Win.WinFrame.width * Win.WinFrame.height);
            CovarianceMatrix[2, 2] = BlueMean / (Win.WinFrame.width * Win.WinFrame.height);
            return CovarianceMatrix;
        }

    }
}
