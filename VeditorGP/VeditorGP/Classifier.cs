using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
//using Emgu.CV.ML;
//using Emgu.CV.Structure;
//using Accord.Math;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics.Distributions.Multivariate;

namespace VeditorGP
{
    //MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
    class Classifier
    {
        #region Variables and constructors
        public Classifier(Window _Window)
        {
            MyWindow = _Window;
        }
        Window MyWindow;
        List<Point> ForegroundPoints, BackgroundPoints;
        double[] ForegroundKmean1, ForegroundKmean2, ForegroundKmean3, BackgroundKmean1, BackgroundKmean2, BackgroundKmean3;
        double[,] BackgroundKmeanCovarinace1, BackgroundKmeanCovarinace2, BackgroundKmeanCovarinace3,
             ForegroundKmeanCovarinace1, ForegroundKmeanCovarinace2, ForegroundKmeanCovarinace3
             , BackgroundEMCovariance1, BackgroundEMCovariance2, BackgroundEMCovariance3
             , ForegroundEMCovariance1, ForegroundEMCovariance2, ForegroundEMCovariance3;
        double[] BackgroundEMmean1, BackgroundEMmean2, BackgroundEMmean3, ForegroundEMmean1, ForegroundEMmean2, ForegroundEMmean3
            , ForeGroundProbability, BackGroundProbability;
        #endregion

        #region Training Step
        public void Train()
        {
            DividData();
            ForegroundKmean();
            BackgroundKmean();
            ForegrounEM();
            BackgrounEM();
            OurGMM(); //P(x)
            CalculateColorConfidence();
        }
        void CalculateColorConfidence()
        {
            int width = MyWindow.WindowFrame.width, height = MyWindow.WindowFrame.height, PointCount = MyWindow.WindowContourPoints.Count;
            double SegmentationLabel = 0, Summation = 0.0, WeightsSummation = 0.0, SigmaCSquare = 202500.0;//sigma square
            double Distance = double.MaxValue, Temp, inResult;
            MyWindow.WeightingFunction = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    foreach (Point  item in MyWindow.WindowContourPoints)
                    {
                        Temp = Math.Sqrt(Math.Pow((item.X - i), 2) + Math.Pow((item.Y - j), 2));
                        if (Temp < Distance)
                            Distance = Temp;
                    }
                    inResult = Math.Pow(Distance,2);
                    inResult = -1 * inResult;
                    inResult /= SigmaCSquare;
                    MyWindow.WeightingFunction[i, j] = Math.Exp(inResult);
                }
            }
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    if (MyWindow.WindowBinaryMask.byteRedPixels[i, j] == 255)
                        SegmentationLabel = 1;
                    Summation += (Math.Abs(SegmentationLabel - MyWindow.ForegroundProbability[i, j]) * MyWindow.WeightingFunction[i, j]);
                    WeightsSummation += MyWindow.WeightingFunction[i, j];
                }
            MyWindow.ColorConfidence = Summation / WeightsSummation;
            MyWindow.ColorConfidence = 1 - MyWindow.ColorConfidence;
        }
        void OurGMM()
        {
            double[] Sample = new double[3];
            ForeGroundProbability = new double[3];
            BackGroundProbability = new double[3];
            int width = MyWindow.WindowFrame.width, height = MyWindow.WindowFrame.height;
            MyWindow.ForegroundProbability = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Sample[0] = MyWindow.WindowFrame.doubleRedPixels[i, j];
                    Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[i, j];
                    Sample[2] = MyWindow.WindowFrame.doubleBluePixels[i, j];

                    #region K-mean
                    //ForeGroundProbability[0] = MultivariateNormalGaussian(Sample, ForegroundKmean1, ForegroundKmeanCovarinace1);
                    //ForeGroundProbability[0] /= 3;
                    //ForeGroundProbability[1] = MultivariateNormalGaussian(Sample, ForegroundKmean2, ForegroundKmeanCovarinace2);
                    //ForeGroundProbability[1] /= 3;
                    //ForeGroundProbability[2] = MultivariateNormalGaussian(Sample, ForegroundKmean3, ForegroundKmeanCovarinace3);
                    //ForeGroundProbability[2] /= 3;

                    //BackGroundProbability[0] = MultivariateNormalGaussian(Sample, BackgroundKmean1, BackgroundKmeanCovarinace1);
                    //BackGroundProbability[0] /= 3;
                    //BackGroundProbability[1] = MultivariateNormalGaussian(Sample, BackgroundKmean2, BackgroundKmeanCovarinace2);
                    //BackGroundProbability[1] /= 3;
                    //BackGroundProbability[2] = MultivariateNormalGaussian(Sample, BackgroundKmean3, BackgroundKmeanCovarinace3);
                    //BackGroundProbability[2] /= 3; 
                    #endregion
                    #region EM
                    ForeGroundProbability[0] = MultivariateNormalGaussian(Sample, ForegroundEMmean1, ForegroundEMCovariance1);
                    ForeGroundProbability[0] /= 3;
                    ForeGroundProbability[1] = MultivariateNormalGaussian(Sample, ForegroundEMmean2, ForegroundEMCovariance2);
                    ForeGroundProbability[1] /= 3;
                    ForeGroundProbability[2] = MultivariateNormalGaussian(Sample, ForegroundEMmean3, ForegroundEMCovariance3);
                    ForeGroundProbability[2] /= 3;

                    BackGroundProbability[0] = MultivariateNormalGaussian(Sample, BackgroundEMmean1, BackgroundEMCovariance1);
                    BackGroundProbability[0] /= 3;
                    BackGroundProbability[1] = MultivariateNormalGaussian(Sample, BackgroundEMmean2, BackgroundEMCovariance2);
                    BackGroundProbability[1] /= 3;
                    BackGroundProbability[2] = MultivariateNormalGaussian(Sample, BackgroundEMmean3, BackgroundEMCovariance3);
                    BackGroundProbability[2] /= 3; 
                    #endregion

                    MyWindow.ForegroundProbability[i, j] = (ForeGroundProbability[0] + ForeGroundProbability[1] + ForeGroundProbability[2]) /
                        (ForeGroundProbability[0] + ForeGroundProbability[1] + ForeGroundProbability[2] +
                        BackGroundProbability[0] + BackGroundProbability[1] + BackGroundProbability[2]);
                    //if (MyWindow.ForegroundProbability[i, j] * 100 > 50)
                    //    MyWindow.ForegroundProbability[i, j] = 255;
                    //else
                    //    MyWindow.ForegroundProbability[i, j] = 0;
                }
            }
            #region Fc

            #endregion
        }
        double MultivariateNormalGaussian(double[] X, double[] Mu, double[,] Sigma)
        {
            double Result = 0.0;
            double Term = 1 / Math.Pow((2 * Math.PI), 3 / 2);
            double Determ = MatrixDet(ForegroundKmeanCovarinace1);
            Term *= Math.Pow(Determ, 1 / 2);
            double[] Difference = new double[3];
            for (int i = 0; i < 3; i++)
                Difference[i] = X[i] - Mu[i];
            double[,] SigmaInverse = Matrix.Inverse(Sigma);
            double[] MulRes = new double[3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    MulRes[i] += (Difference[i] * SigmaInverse[j, i]);
            double Temp = 0.0;
            for (int i = 0; i < 3; i++)
                Temp += (Difference[i] * MulRes[i]);
            Temp *= -0.5;
            Result = Term * Math.Exp(Temp);
            return Result;
        }
        double MatrixDet(double[,] SubMatrix)
        {
            double res = 0;
            if (SubMatrix.Length == 4)
                res += SubMatrix[0, 0] * SubMatrix[1, 1] - SubMatrix[0, 1] * SubMatrix[1, 0];
            else
            {
                int Count = (int)Math.Sqrt(SubMatrix.Length);
                double[,] SmallMatrix = new double[Count - 1, Count - 1];
                for (int i = 0; i < 1; i++)
                    for (int j = 0; j < Count; j++)
                    {
                        for (int v = 0, c = 0; c < Count; c++)
                            for (int k = 0, p = 0; p < Count; p++)
                                if (c != i && p != j)
                                {
                                    SmallMatrix[v, k++] = SubMatrix[c, p];
                                    if (k == Count - 1)
                                    {
                                        k = 0;
                                        v++;
                                    }
                                    if (v == Count - 1)
                                        break;
                                }
                        res += (SubMatrix[i, j] * (int)Math.Pow(-1, (i + j)) * MatrixDet(SmallMatrix));
                    }
            }
            return res;
        }
        int[] TestEqualCentroids(List<double[]> centroids)
        {
            int[] Result = new int[3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (i != j)
                        Result[i] = (int)Distance(centroids[i], centroids[j]);
            return Result;
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
        void BackgrounEM()
        {
            Random r = new Random(DateTime.Now.Millisecond);
            int length = BackgroundPoints.Count;//3adad el data set
            int Dimention = 3;

            EM Em = new EM();
            Matrix<int> labels = new Matrix<int>(length, 1);
            Matrix<float> featuresM = new Matrix<float>(length, Dimention);
            int PatternCount = 0;
            for (int i = 0; i < length; i++)
            {
                featuresM[PatternCount, 0] = (float)MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                featuresM[PatternCount, 1] = (float)MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                featuresM[PatternCount, 2] = (float)MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                PatternCount++;
            }

            EMParams pars = new EMParams();
            Matrix<double>[] Covariances = new Matrix<double>[3];
            Covariances[0] = new Matrix<double>(BackgroundKmeanCovarinace1);
            Covariances[1] = new Matrix<double>(BackgroundKmeanCovarinace2);
            Covariances[2] = new Matrix<double>(BackgroundKmeanCovarinace3);
            pars.Covs = Covariances;
            double[,] OurKmeans = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                OurKmeans[0, i] = BackgroundKmean1[i];
                OurKmeans[1, i] = BackgroundKmean2[i];
                OurKmeans[2, i] = BackgroundKmean3[i];
            }
            pars.Means = new Matrix<double>(OurKmeans);
            pars.Nclusters = 3;
            pars.StartStep = Emgu.CV.ML.MlEnum.EM_INIT_STEP_TYPE.START_AUTO_STEP;
            pars.TermCrit = new MCvTermCriteria(100, 1.0e-6);

            Em.Train(featuresM, null, pars, labels);
            IntPtr Means = Em.Means.MCvMat.data;
            Matrix<double> NewMeans = pars.Means;
            BackgroundEMmean1 = new double[3];
            BackgroundEMmean2 = new double[3];
            BackgroundEMmean3 = new double[3];
            for (int i = 0; i < 3; i++)
            {
                BackgroundEMmean1[i] = NewMeans.Data[0, i];
                BackgroundEMmean2[i] = NewMeans.Data[1, i];
                BackgroundEMmean3[i] = NewMeans.Data[2, i];
            }
            Matrix<double>[] Covariance = pars.Covs;
            BackgroundEMCovariance1 = Covariance[0].Data;
            BackgroundEMCovariance2 = Covariance[1].Data;
            BackgroundEMCovariance3 = Covariance[2].Data;
        }
        void ForegrounEM()
        {
            Random r = new Random(DateTime.Now.Millisecond);
            int length = ForegroundPoints.Count;//3adad el data set
            int Dimention = 3;

            EM Em = new EM();
            Matrix<int> labels = new Matrix<int>(length, 1);
            Matrix<float> featuresM = new Matrix<float>(length, Dimention);
            int PatternCount = 0;
            for (int i = 0; i < length; i++)
            {
                featuresM[PatternCount, 0] = (float)MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                featuresM[PatternCount, 1] = (float)MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                featuresM[PatternCount, 2] = (float)MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                PatternCount++;
            }

            EMParams pars = new EMParams();
            Matrix<double>[] Covariances = new Matrix<double>[3];
            Covariances[0] = new Matrix<double>(ForegroundKmeanCovarinace1);
            Covariances[1] = new Matrix<double>(ForegroundKmeanCovarinace2);
            Covariances[2] = new Matrix<double>(ForegroundKmeanCovarinace3);
            pars.Covs = Covariances;
            double[,] OurKmeans = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                OurKmeans[0, i] = ForegroundKmean1[i];
                OurKmeans[1, i] = ForegroundKmean2[i];
                OurKmeans[2, i] = ForegroundKmean3[i];
            }
            pars.Means = new Matrix<double>(OurKmeans);
            pars.Nclusters = 3;
            pars.StartStep = Emgu.CV.ML.MlEnum.EM_INIT_STEP_TYPE.START_AUTO_STEP;
            pars.TermCrit = new MCvTermCriteria(100, 1.0e-6);

            Em.Train(featuresM, null, pars, labels);
            IntPtr Means = Em.Means.MCvMat.data;
            Matrix<double> NewMeans = pars.Means;
            ForegroundEMmean1 = new double[3];
            ForegroundEMmean2 = new double[3];
            ForegroundEMmean3 = new double[3];
            for (int i = 0; i < 3; i++)
            {
                ForegroundEMmean1[i] = NewMeans.Data[0, i];
                ForegroundEMmean2[i] = NewMeans.Data[1, i];
                ForegroundEMmean3[i] = NewMeans.Data[2, i];
            }
            Matrix<double>[] Covariance = pars.Covs;
            ForegroundEMCovariance1 = Covariance[0].Data;
            ForegroundEMCovariance2 = Covariance[1].Data;
            ForegroundEMCovariance3 = Covariance[2].Data;
        }
        void BackgroundKmean()
        {
            int Classes = 3;
            int BackGroundLength = BackgroundPoints.Count;

            #region Get Random Centroids
            List<double[]> centroids = new List<double[]>();

            Random rand = new Random();

            for (int k = 0; k < Classes; k++)
            {
                int i = rand.Next(0, BackGroundLength);
                double[] temp = new double[3];
                temp[0] = MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                temp[1] = MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                temp[2] = MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                centroids.Add(temp);
            }
            while (true)
            {
                int[] isEqual = TestEqualCentroids(centroids);
                if (isEqual[0] == 0 && isEqual[1] == 0 && isEqual[2] == 0)
                    break;
                centroids.Clear();
                for (int k = 0; k < Classes; k++)
                {
                    int i = rand.Next(0, BackGroundLength);
                    double[] temp = new double[3];
                    temp[0] = MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                    temp[1] = MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                    temp[2] = MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
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
                List<double[]> Mean3Vectors = new List<double[]>();
                for (int i = 0; i < BackGroundLength; i++)
                {
                    double[] DataVector = new double[3];
                    DataVector[0] = MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                    DataVector[1] = MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                    DataVector[2] = MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                    double Distance1 = Distance(DataVector, centroids[0]);
                    double Distance2 = Distance(DataVector, centroids[1]);
                    double Distance3 = Distance(DataVector, centroids[2]);
                    if (Distance1 < Distance2 && Distance1 < Distance3)
                        Mean1Vectors.Add(DataVector);
                    else if (Distance2 < Distance1 && Distance2 < Distance3)
                        Mean2Vectors.Add(DataVector);
                    else
                        Mean3Vectors.Add(DataVector);
                }
                change = false;
                double[] NewMean1 = FindNewMean(Mean1Vectors);
                double[] NewMean2 = FindNewMean(Mean2Vectors);
                double[] NewMean3 = FindNewMean(Mean3Vectors);
                double TempDiff1 = Distance(NewMean1, centroids[0]);
                double TempDiff2 = Distance(NewMean2, centroids[1]);
                double TempDiff3 = Distance(NewMean3, centroids[2]);
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
                if (TempDiff3 > 0.001)
                {
                    centroids[2] = NewMean3;
                    change = true;
                }
            }
            #endregion

            BackgroundKmean1 = centroids[0];
            BackgroundKmean2 = centroids[1];
            BackgroundKmean3 = centroids[2];
            BackgroundKmeanCovarinace1 = GetCovariance(BackgroundKmean1, 1);
            BackgroundKmeanCovarinace2 = GetCovariance(BackgroundKmean2, 1);
            BackgroundKmeanCovarinace3 = GetCovariance(BackgroundKmean3, 1);
        }
        void ForegroundKmean()
        {
            int Classes = 3;
            int ForeGroundLength = ForegroundPoints.Count;

            #region Get Random Centroids
            List<double[]> centroids = new List<double[]>();

            Random rand = new Random();

            for (int k = 0; k < Classes; k++)
            {
                int i = rand.Next(0, ForeGroundLength);
                double[] temp = new double[3];
                temp[0] = MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                temp[1] = MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                temp[2] = MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                centroids.Add(temp);
            }
            while (true)
            {
                int[] isEqual = TestEqualCentroids(centroids);
                if (isEqual[0] != 0 && isEqual[1] != 0 && isEqual[2] != 0)
                    break;
                centroids.Clear();
                for (int k = 0; k < Classes; k++)
                {
                    int i = rand.Next(0, ForeGroundLength);
                    double[] temp = new double[3];
                    temp[0] = MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                    temp[1] = MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                    temp[2] = MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
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
                List<double[]> Mean3Vectors = new List<double[]>();
                for (int i = 0; i < ForeGroundLength; i++)
                {
                    double[] DataVector = new double[3];
                    DataVector[0] = MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                    DataVector[1] = MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                    DataVector[2] = MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                    double Distance1 = Distance(DataVector, centroids[0]);
                    double Distance2 = Distance(DataVector, centroids[1]);
                    double Distance3 = Distance(DataVector, centroids[2]);
                    if (Distance1 < Distance2 && Distance1 < Distance3)
                        Mean1Vectors.Add(DataVector);
                    else if (Distance2 < Distance1 && Distance2 < Distance3)
                        Mean2Vectors.Add(DataVector);
                    else
                        Mean3Vectors.Add(DataVector);
                }
                change = false;
                double[] NewMean1 = FindNewMean(Mean1Vectors);
                double[] NewMean2 = FindNewMean(Mean2Vectors);
                double[] NewMean3 = FindNewMean(Mean3Vectors);
                double TempDiff1 = Distance(NewMean1, centroids[0]);
                double TempDiff2 = Distance(NewMean2, centroids[1]);
                double TempDiff3 = Distance(NewMean3, centroids[2]);
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
                if (TempDiff3 > 0.001)
                {
                    centroids[2] = NewMean3;
                    change = true;
                }
            }
            #endregion

            ForegroundKmean1 = centroids[0];
            ForegroundKmean2 = centroids[1];
            ForegroundKmean3 = centroids[2];
            ForegroundKmeanCovarinace1 = GetCovariance(ForegroundKmean1, 0);
            ForegroundKmeanCovarinace2 = GetCovariance(ForegroundKmean2, 0);
            ForegroundKmeanCovarinace3 = GetCovariance(ForegroundKmean3, 0);
        }
        double[,] GetCovariance(double[] _MeanMatrix, int Area)
        {
            double[,] CovarianceMatrix = new double[3, 3];
            double ReadMean = 0, GreenMean = 0, BlueMean = 0;
            int length;
            if (Area == 0) //foreground
            {
                length = ForegroundPoints.Count;
                for (int i = 0; i < length; i++)
                {
                    ReadMean += Math.Pow((MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y] - _MeanMatrix[0]), 2);
                    GreenMean += Math.Pow((MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y] - _MeanMatrix[1]), 2);
                    BlueMean += Math.Pow((MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y] - _MeanMatrix[2]), 2);
                }
            }
            else
            {
                length = BackgroundPoints.Count;
                for (int i = 0; i < length; i++)
                {
                    ReadMean += Math.Pow((MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y] - _MeanMatrix[0]), 2);
                    GreenMean += Math.Pow((MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y] - _MeanMatrix[1]), 2);
                    BlueMean += Math.Pow((MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y] - _MeanMatrix[2]), 2);
                }
            }
            CovarianceMatrix[0, 0] = ReadMean / length;
            CovarianceMatrix[1, 1] = GreenMean / length;
            CovarianceMatrix[2, 2] = BlueMean / length;
            return CovarianceMatrix;
        }
        void DividData()
        {
            int Width = MyWindow.WindowFrame.width, Height = MyWindow.WindowFrame.height;
            ForegroundPoints = new List<Point>();
            BackgroundPoints = new List<Point>();
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    if (MyWindow.WindowBinaryMask.byteRedPixels[i, j] == 0)
                        BackgroundPoints.Add(new Point(j, i));
                    else
                        ForegroundPoints.Add(new Point(j, i));
                }
        }
        #endregion
    }
}
