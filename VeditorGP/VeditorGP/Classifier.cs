using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Accord.Math;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using openCV;

namespace VeditorGP
{
    class Classifier
    {
        #region Variables and constructors
        bool FTrain1 = true, FTrain2 = true, FTrain3 = true;
        bool BTrain1 = true, BTrain2 = true, BTrain3 = true;
        public Classifier(Window _Window)
        {
            MyWindow = _Window;
        }
        Window MyWindow;
        List<Point> ForegroundPoints, BackgroundPoints;
        double TINY = 0.0000000001;
        double[] FWeights, BWeights;
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
            OurForegroundEM();
            OurBackgroundEM();
            
            OurGMM(); //P(x)
        }
        void OurGMM()
        {
            double[] Sample = new double[3];
            int width = MyWindow.WindowFrame.width, height = MyWindow.WindowFrame.height;
            byte[,] TempCalssify = new byte[height, width];
            MyWindow.ForegroundProbability = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    ForeGroundProbability = new double[3];
                    BackGroundProbability = new double[3];
                    Sample[0] = MyWindow.WindowFrame.doubleRedPixels[i, j];
                    Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[i, j];
                    Sample[2] = MyWindow.WindowFrame.doubleBluePixels[i, j];
                    #region EM
                    if (FTrain1)
                    {
                        ForeGroundProbability[0] = MultivariateNormalGaussian(Sample, ForegroundEMmean1, ForegroundEMCovariance1);
                        ForeGroundProbability[0] *= FWeights[0];
                    }
                    if (FTrain2)
                    {
                        ForeGroundProbability[1] = MultivariateNormalGaussian(Sample, ForegroundEMmean2, ForegroundEMCovariance2);
                        ForeGroundProbability[1] *= FWeights[1];
                    }
                    if (FTrain3)
                    {
                        ForeGroundProbability[2] = MultivariateNormalGaussian(Sample, ForegroundEMmean3, ForegroundEMCovariance3);
                        ForeGroundProbability[2] *= FWeights[2];
                    }
                    if (BTrain1)
                    {
                        BackGroundProbability[0] = MultivariateNormalGaussian(Sample, BackgroundEMmean1, BackgroundEMCovariance1);
                        BackGroundProbability[0] *= BWeights[0];
                    }
                    if (BTrain2)
                    {
                        BackGroundProbability[1] = MultivariateNormalGaussian(Sample, BackgroundEMmean2, BackgroundEMCovariance2);
                        BackGroundProbability[1] *= BWeights[1];
                    }
                    if (BTrain3)
                    {
                        BackGroundProbability[2] = MultivariateNormalGaussian(Sample, BackgroundEMmean3, BackgroundEMCovariance3);
                        BackGroundProbability[2] *= BWeights[2];
                    }
                    #endregion
                    MyWindow.ForegroundProbability[i, j] = (ForeGroundProbability[0] + ForeGroundProbability[1] + ForeGroundProbability[2]) /
                        (ForeGroundProbability[0] + ForeGroundProbability[1] + ForeGroundProbability[2] +
                        BackGroundProbability[0] + BackGroundProbability[1] + BackGroundProbability[2]);
                    if (MyWindow.ForegroundProbability[i, j] < 0.5)
                        TempCalssify[i, j] = 0;
                    else
                        TempCalssify[i, j] = 255;
                }
            }
            #region Test Saving Boundary Image
            Bitmap NewImage = new Bitmap(width, height);
            BitmapData bmpData = NewImage.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, NewImage.PixelFormat);
            unsafe
            {
                byte* p = (byte*)bmpData.Scan0;
                int space = bmpData.Stride - width * 3;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        p[0] = TempCalssify[i, j];
                        p[1] = TempCalssify[i, j];
                        p[2] = TempCalssify[i, j];
                        p += 3;
                    }
                    p += space;
                }
            }
            NewImage.UnlockBits(bmpData);
            string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Classifier.bmp";
            NewImage.Save(Pw, ImageFormat.Bmp);
            #endregion
            #region Fc

            #endregion
        }
        double MultivariateNormalGaussian(double[] X, double[] Mu, double[,] Sigma)
        {
            double Result = 0.0;
            double Term = 1 / Math.Pow((2 * Math.PI), 3 / 2);
            double Determ = MatrixDet(Sigma);
            Term *= Math.Pow(Determ, 1 / 2);
            //if (Determ == 0.0)
            //    Determ = TINY;
            double[] Difference = new double[3];
            for (int i = 0; i < 3; i++)
                Difference[i] = X[i] - Mu[i];
            //if (Sigma[0, 0] < TINY )
            //    Sigma[0, 0] = TINY;
            //if (Sigma[1, 1] < TINY)
            //    Sigma[1, 1] = TINY;
            //if (Sigma[2, 2] < TINY)
            //    Sigma[2, 2] = TINY;
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
        void OurForegroundEM()
        {
            #region Initialization and Variables
            double[] OldMu1, OldMu2, OldMu3, TempWeights;
            double[,] OldSigma1, OldSigma2, OldSigma3;
            OldMu1 = new double[3];
            OldMu2 = new double[3];
            OldMu3 = new double[3];
            OldSigma1 = new double[3, 3];
            OldSigma2 = new double[3, 3];
            OldSigma3 = new double[3, 3];
            ForegroundEMCovariance1 = ForegroundKmeanCovarinace1;
            ForegroundEMCovariance2 = ForegroundKmeanCovarinace2;
            ForegroundEMCovariance3 = ForegroundKmeanCovarinace3;
            FWeights = new double[3];
            FWeights[0] = 1.0 / 3.0;
            FWeights[1] = 1.0 / 3.0;
            FWeights[2] = 1.0 / 3.0;
            int ForegroundCount = ForegroundPoints.Count;
            double SummationComponent1 = 0.0, SummationComponent2 = 0.0, SummationComponent3 = 0.0;
            double Prior1, Prior2, Prior3;
            double[] Sample;
            ForegroundEMmean1 = ForegroundKmean1;
            ForegroundEMmean2 = ForegroundKmean2;
            ForegroundEMmean3 = ForegroundKmean3;
            #endregion

            #region iterations
            while ((Distance(OldMu1, ForegroundEMmean1) > 0.01 && FWeights[0] != 0) || (Distance(OldMu2, ForegroundEMmean2) > 0.01 && FWeights[1] != 0) || (Distance(OldMu3, ForegroundEMmean3) > 0.01 && FWeights[2] != 0))
            {
                TempWeights = new double[3];
                FWeights.CopyTo(TempWeights, 0);
                #region Simga 2araf
                if (ForegroundEMCovariance1[0, 0] == 0 || double.IsNaN(ForegroundEMCovariance1[0, 0]) || ForegroundEMCovariance1[1, 1] == 0 || ForegroundEMCovariance1[2, 2] == 0)
                {
                    FTrain1 = false;
                    FWeights[0] = 0;
                }
                else
                {
                    OldMu1 = new double[3];
                    OldSigma1 = new double[3, 3];
                    OldSigma1 = ForegroundEMCovariance1;
                    ForegroundEMmean1.CopyTo(OldMu1, 0);
                    ForegroundEMCovariance1 = new double[3, 3];
                    ForegroundEMmean1 = new double[3];
                    SummationComponent1 = 0.0;
                }
                if (ForegroundEMCovariance2[0, 0] == 0 || double.IsNaN(ForegroundEMCovariance2[0, 0]) || ForegroundEMCovariance2[1, 1] == 0 || ForegroundEMCovariance2[2, 2] == 0)
                {
                    FTrain2 = false;
                    FWeights[1] = 0;
                }
                else
                {
                    OldMu2 = new double[3];
                    OldSigma2 = new double[3, 3];
                    OldSigma2 = ForegroundEMCovariance2;
                    ForegroundEMmean2.CopyTo(OldMu2, 0);
                    ForegroundEMCovariance2 = new double[3, 3];
                    ForegroundEMmean2 = new double[3];
                    SummationComponent2 = 0.0;
                }
                if (ForegroundEMCovariance3[0, 0] == 0 || double.IsNaN(ForegroundEMCovariance3[0, 0]) || ForegroundEMCovariance3[1, 1] == 0 || ForegroundEMCovariance3[2, 2] == 0)
                {
                    FTrain3 = false;
                    FWeights[2] = 0;
                }
                else
                {
                    OldMu3 = new double[3];
                    OldSigma3 = new double[3, 3];
                    ForegroundEMmean3.CopyTo(OldMu3, 0);
                    OldSigma3 = ForegroundEMCovariance3;
                    ForegroundEMCovariance3 = new double[3, 3];
                    ForegroundEMmean3 = new double[3];
                    SummationComponent3 = 0.0;
                }
                #endregion
                #region Train First Component
                if (FTrain1)
                {
                    for (int i = 0; i < ForegroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Prior1 = PriorProbability(0, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        SummationComponent1 += Prior1;
                        ForegroundEMmean1[0] += (Prior1 * Sample[0]);
                        ForegroundEMmean1[1] += (Prior1 * Sample[1]);
                        ForegroundEMmean1[2] += (Prior1 * Sample[2]);
                    }
                    FWeights[0] = SummationComponent1 / ForegroundCount;
                    ForegroundEMmean1[0] /= SummationComponent1;
                    ForegroundEMmean1[1] /= SummationComponent1;
                    ForegroundEMmean1[2] /= SummationComponent1;
                    #region Sigma
                    for (int i = 0; i < ForegroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Prior1 = PriorProbability(0, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        ForegroundEMCovariance1[0, 0] += (Prior1 * Math.Pow((Sample[0] - ForegroundEMmean1[0]), 2));
                        ForegroundEMCovariance1[1, 1] += (Prior1 * Math.Pow((Sample[1] - ForegroundEMmean1[1]), 2));
                        ForegroundEMCovariance1[2, 2] += (Prior1 * Math.Pow((Sample[2] - ForegroundEMmean1[2]), 2));
                    }
                    ForegroundEMCovariance1[0, 0] /= SummationComponent1;
                    ForegroundEMCovariance1[1, 1] /= SummationComponent1;
                    ForegroundEMCovariance1[2, 2] /= SummationComponent1;
                    #endregion
                }
                #endregion
                #region Train Second Component
                if (FTrain2)
                {
                    for (int i = 0; i < ForegroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Prior2 = PriorProbability(1, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        SummationComponent2 += Prior2;
                        ForegroundEMmean2[0] += (Prior2 * Sample[0]);
                        ForegroundEMmean2[1] += (Prior2 * Sample[1]);
                        ForegroundEMmean2[2] += (Prior2 * Sample[2]);
                    }
                    FWeights[1] = SummationComponent2 / ForegroundCount;
                    ForegroundEMmean2[0] /= SummationComponent2;
                    ForegroundEMmean2[1] /= SummationComponent2;
                    ForegroundEMmean2[2] /= SummationComponent2;
                    #region Sigma
                    for (int i = 0; i < ForegroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Prior2 = PriorProbability(1, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        ForegroundEMCovariance2[0, 0] += (Prior2 * Math.Pow((Sample[0] - ForegroundEMmean2[0]), 2));
                        ForegroundEMCovariance2[1, 1] += (Prior2 * Math.Pow((Sample[1] - ForegroundEMmean2[1]), 2));
                        ForegroundEMCovariance2[2, 2] += (Prior2 * Math.Pow((Sample[2] - ForegroundEMmean2[2]), 2));
                    }
                    ForegroundEMCovariance2[0, 0] /= SummationComponent2;
                    ForegroundEMCovariance2[1, 1] /= SummationComponent2;
                    ForegroundEMCovariance2[2, 2] /= SummationComponent2;
                    #endregion
                }
                #endregion
                #region Train Third Component
                if (FTrain3)
                {
                    for (int i = 0; i < ForegroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Prior3 = PriorProbability(2, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        SummationComponent3 += Prior3;
                        ForegroundEMmean3[0] += (Prior3 * Sample[0]);
                        ForegroundEMmean3[1] += (Prior3 * Sample[1]);
                        ForegroundEMmean3[2] += (Prior3 * Sample[2]);
                    }
                    FWeights[2] = SummationComponent3 / ForegroundCount;
                    ForegroundEMmean3[0] /= SummationComponent3;
                    ForegroundEMmean3[1] /= SummationComponent3;
                    ForegroundEMmean3[2] /= SummationComponent3;
                    #region Sigma
                    for (int i = 0; i < ForegroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[ForegroundPoints[i].X, ForegroundPoints[i].Y];
                        Prior3 = PriorProbability(2, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        ForegroundEMCovariance3[0, 0] += (Prior3 * Math.Pow((Sample[0] - ForegroundEMmean3[0]), 2));
                        ForegroundEMCovariance3[1, 1] += (Prior3 * Math.Pow((Sample[1] - ForegroundEMmean3[1]), 2));
                        ForegroundEMCovariance3[2, 2] += (Prior3 * Math.Pow((Sample[2] - ForegroundEMmean3[2]), 2));
                    }
                    ForegroundEMCovariance3[0, 0] /= SummationComponent3;
                    ForegroundEMCovariance3[1, 1] /= SummationComponent3;
                    ForegroundEMCovariance3[2, 2] /= SummationComponent3;
                    #endregion
                }
                #endregion
            }
            #endregion
            if (ForegroundEMCovariance1[0, 0] == 0 || double.IsNaN(ForegroundEMCovariance1[0, 0]) || ForegroundEMCovariance1[1, 1] == 0 || ForegroundEMCovariance1[2, 2] == 0)
            {
                FTrain1 = false;
                FWeights[0] = 0;
            }
            if (ForegroundEMCovariance2[0, 0] == 0 || double.IsNaN(ForegroundEMCovariance2[0, 0]) || ForegroundEMCovariance2[1, 1] == 0 || ForegroundEMCovariance2[2, 2] == 0)
            {
                FTrain2 = false;
                FWeights[1] = 0;
            }
            if (ForegroundEMCovariance3[0, 0] == 0 || double.IsNaN(ForegroundEMCovariance3[0, 0]) || ForegroundEMCovariance3[1, 1] == 0 || ForegroundEMCovariance3[2, 2] == 0)
            {
                FTrain3 = false;
                FWeights[2] = 0;
            }
        }
        void OurBackgroundEM()
        {
            #region Initialization and variables
            double[] OldMu1, OldMu2, OldMu3, TempWeights;
            double[,] OldSigma1, OldSigma2, OldSigma3;
            OldMu1 = new double[3];
            OldMu2 = new double[3];
            OldMu3 = new double[3];
            OldSigma1 = new double[3, 3];
            OldSigma2 = new double[3, 3];
            OldSigma3 = new double[3, 3];
            BackgroundEMCovariance1 = BackgroundKmeanCovarinace1;
            BackgroundEMCovariance2 = BackgroundKmeanCovarinace2;
            BackgroundEMCovariance3 = BackgroundKmeanCovarinace3;
            BWeights = new double[3];
            BWeights[0] = 1.0 / 3.0;
            BWeights[1] = 1.0 / 3.0;
            BWeights[2] = 1.0 / 3.0;
            int BackgroundCount = BackgroundPoints.Count;
            double SummationComponent1 = 0.0, SummationComponent2 = 0.0, SummationComponent3 = 0.0;
            double Prior1, Prior2, Prior3;
            double[] Sample;
            BackgroundEMmean1 = BackgroundKmean1;
            BackgroundEMmean2 = BackgroundKmean2;
            BackgroundEMmean3 = BackgroundKmean3;
            #endregion

            #region iterations
            while ((Distance(OldMu1, BackgroundEMmean1) > 0.01 && BWeights[0] != 0) || (Distance(OldMu2, BackgroundEMmean2) > 0.01 && BWeights[1] != 0) || (Distance(OldMu3, BackgroundEMmean3) > 0.01 && BWeights[2] != 0))
            {
                TempWeights = new double[3];
                BWeights.CopyTo(TempWeights, 0);
                #region Simga 2araf
                if (BackgroundEMCovariance1[0, 0] == 0 || double.IsNaN(BackgroundEMCovariance1[0, 0]) || BackgroundEMCovariance1[1, 1] == 0 || BackgroundEMCovariance1[2, 2] == 0)
                {
                    BTrain1 = false;
                    BWeights[0] = 0;
                }
                else
                {
                    OldMu1 = new double[3];
                    OldSigma1 = new double[3, 3];
                    OldSigma1 = BackgroundEMCovariance1;
                    BackgroundEMmean1.CopyTo(OldMu1, 0);
                    BackgroundEMCovariance1 = new double[3, 3];
                    BackgroundEMmean1 = new double[3];
                    SummationComponent1 = 0.0;
                }
                if (BackgroundEMCovariance2[0, 0] == 0 || double.IsNaN(BackgroundEMCovariance2[0, 0]) || BackgroundEMCovariance2[1, 1] == 0 || BackgroundEMCovariance2[2, 2] == 0)
                {
                    BTrain2 = false;
                    BWeights[1] = 0;
                }
                else
                {
                    OldMu2 = new double[3];
                    OldSigma2 = new double[3, 3];
                    OldSigma2 = BackgroundEMCovariance2;
                    BackgroundEMmean2.CopyTo(OldMu2, 0);
                    BackgroundEMCovariance2 = new double[3, 3];
                    BackgroundEMmean2 = new double[3];
                    SummationComponent2 = 0.0;
                }
                if (BackgroundEMCovariance3[0, 0] == 0 || double.IsNaN(BackgroundEMCovariance3[0, 0]) || BackgroundEMCovariance3[1, 1] == 0 || BackgroundEMCovariance3[2, 2] == 0)
                {
                    BTrain3 = false;
                    BWeights[2] = 0;
                }
                else
                {
                    OldMu3 = new double[3];
                    OldSigma3 = new double[3, 3];
                    BackgroundEMmean3.CopyTo(OldMu3, 0);
                    OldSigma3 = BackgroundEMCovariance3;
                    BackgroundEMCovariance3 = new double[3, 3];
                    BackgroundEMmean3 = new double[3];
                    SummationComponent3 = 0.0;
                }
                #endregion
                #region Train First Component
                if (BTrain1)
                {
                    for (int i = 0; i < BackgroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Prior1 = BPriorProbability(0, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        SummationComponent1 += Prior1;
                        BackgroundEMmean1[0] += (Prior1 * Sample[0]);
                        BackgroundEMmean1[1] += (Prior1 * Sample[1]);
                        BackgroundEMmean1[2] += (Prior1 * Sample[2]);
                    }
                    BWeights[0] = SummationComponent1 / BackgroundCount;
                    BackgroundEMmean1[0] /= SummationComponent1;
                    BackgroundEMmean1[1] /= SummationComponent1;
                    BackgroundEMmean1[2] /= SummationComponent1;
                    #region Sigma
                    for (int i = 0; i < BackgroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Prior1 = BPriorProbability(0, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        BackgroundEMCovariance1[0, 0] += (Prior1 * Math.Pow((Sample[0] - BackgroundEMmean1[0]), 2));
                        BackgroundEMCovariance1[1, 1] += (Prior1 * Math.Pow((Sample[1] - BackgroundEMmean1[1]), 2));
                        BackgroundEMCovariance1[2, 2] += (Prior1 * Math.Pow((Sample[2] - BackgroundEMmean1[2]), 2));
                    }
                    BackgroundEMCovariance1[0, 0] /= SummationComponent1;
                    BackgroundEMCovariance1[1, 1] /= SummationComponent1;
                    BackgroundEMCovariance1[2, 2] /= SummationComponent1;
                    #endregion
                }
                #endregion
                #region Train Second Component
                if (BTrain2)
                {
                    for (int i = 0; i < BackgroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Prior2 = BPriorProbability(1, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        SummationComponent2 += Prior2;
                        BackgroundEMmean2[0] += (Prior2 * Sample[0]);
                        BackgroundEMmean2[1] += (Prior2 * Sample[1]);
                        BackgroundEMmean2[2] += (Prior2 * Sample[2]);
                    }
                    BWeights[1] = SummationComponent2 / BackgroundCount;
                    BackgroundEMmean2[0] /= SummationComponent2;
                    BackgroundEMmean2[1] /= SummationComponent2;
                    BackgroundEMmean2[2] /= SummationComponent2;
                    #region Sigma
                    for (int i = 0; i < BackgroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Prior2 = BPriorProbability(1, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        BackgroundEMCovariance2[0, 0] += (Prior2 * Math.Pow((Sample[0] - BackgroundEMmean2[0]), 2));
                        BackgroundEMCovariance2[1, 1] += (Prior2 * Math.Pow((Sample[1] - BackgroundEMmean2[1]), 2));
                        BackgroundEMCovariance2[2, 2] += (Prior2 * Math.Pow((Sample[2] - BackgroundEMmean2[2]), 2));
                    }
                    BackgroundEMCovariance2[0, 0] /= SummationComponent2;
                    BackgroundEMCovariance2[1, 1] /= SummationComponent2;
                    BackgroundEMCovariance2[2, 2] /= SummationComponent2;
                    #endregion
                }
                #endregion
                #region Train Third Component
                if (BTrain3)
                {
                    for (int i = 0; i < BackgroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Prior3 = BPriorProbability(2, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        SummationComponent3 += Prior3;
                        BackgroundEMmean3[0] += (Prior3 * Sample[0]);
                        BackgroundEMmean3[1] += (Prior3 * Sample[1]);
                        BackgroundEMmean3[2] += (Prior3 * Sample[2]);
                    }
                    BWeights[2] = SummationComponent3 / BackgroundCount;
                    BackgroundEMmean3[0] /= SummationComponent3;
                    BackgroundEMmean3[1] /= SummationComponent3;
                    BackgroundEMmean3[2] /= SummationComponent3;
                    #region Sigma
                    for (int i = 0; i < BackgroundCount; i++)
                    {
                        Sample = new double[3];
                        Sample[0] = MyWindow.WindowFrame.doubleRedPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[1] = MyWindow.WindowFrame.doubleGreenPixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Sample[2] = MyWindow.WindowFrame.doubleBluePixels[BackgroundPoints[i].X, BackgroundPoints[i].Y];
                        Prior3 = BPriorProbability(2, Sample, OldMu1, OldMu2, OldMu3, OldSigma1, OldSigma2, OldSigma3, TempWeights);
                        BackgroundEMCovariance3[0, 0] += (Prior3 * Math.Pow((Sample[0] - BackgroundEMmean3[0]), 2));
                        BackgroundEMCovariance3[1, 1] += (Prior3 * Math.Pow((Sample[1] - BackgroundEMmean3[1]), 2));
                        BackgroundEMCovariance3[2, 2] += (Prior3 * Math.Pow((Sample[2] - BackgroundEMmean3[2]), 2));
                    }
                    BackgroundEMCovariance3[0, 0] /= SummationComponent3;
                    BackgroundEMCovariance3[1, 1] /= SummationComponent3;
                    BackgroundEMCovariance3[2, 2] /= SummationComponent3;
                    #endregion
                }
                #endregion
            }
            #endregion
            if (BackgroundEMCovariance1[0, 0] == 0 || double.IsNaN(BackgroundEMCovariance1[0, 0]) || BackgroundEMCovariance1[1, 1] == 0 || BackgroundEMCovariance1[2, 2] == 0)
            {
                BTrain1 = false;
                BWeights[0] = 0;
            }
            if (BackgroundEMCovariance2[0, 0] == 0 || double.IsNaN(BackgroundEMCovariance2[0, 0]) || BackgroundEMCovariance2[1, 1] == 0 || BackgroundEMCovariance2[2, 2] == 0)
            {
                BTrain2 = false;
                BWeights[1] = 0;
            }
            if (BackgroundEMCovariance3[0, 0] == 0 || double.IsNaN(BackgroundEMCovariance3[0, 0]) || BackgroundEMCovariance3[1, 1] == 0 || BackgroundEMCovariance3[2, 2] == 0)
            {
                BTrain3 = false;
                BWeights[2] = 0;
            }
        }
        double PriorProbability(int ComponentNumber, double[] TestVector, double[] Mu1, double[] Mu2, double[] Mu3, double[,] Sigma1, double[,] Sigma2, double[,] Sigma3, double[] Weights)
        {
            double result = 0.0, Temp = 0.0;
            if (ComponentNumber == 0)
            {
                Temp = Weights[0] * MultivariateNormalGaussian(TestVector, Mu1, Sigma1);
                result += Temp;
                if (FTrain2)
                    result += (Weights[1] * MultivariateNormalGaussian(TestVector, Mu2, Sigma2));
                if (FTrain3)
                    result += (Weights[2] * MultivariateNormalGaussian(TestVector, Mu3, Sigma3));
            }
            else if (ComponentNumber == 1)
            {
                Temp = Weights[1] * MultivariateNormalGaussian(TestVector, Mu2, Sigma2);
                if (FTrain1)
                    result += (Weights[0] * MultivariateNormalGaussian(TestVector, Mu1, Sigma1));
                result += Temp;
                if (FTrain3)
                    result += (Weights[2] * MultivariateNormalGaussian(TestVector, Mu3, Sigma3));
            }
            else if (ComponentNumber == 2)
            {
                Temp = Weights[2] * MultivariateNormalGaussian(TestVector, Mu3, Sigma3);
                if (FTrain1)
                    result += (Weights[0] * MultivariateNormalGaussian(TestVector, Mu1, Sigma1));
                if (FTrain2)
                    result += (Weights[1] * MultivariateNormalGaussian(TestVector, Mu2, Sigma2));
                result += Temp;
            }
            Temp /= result;
            return Temp;
        }
        double BPriorProbability(int ComponentNumber, double[] TestVector, double[] Mu1, double[] Mu2, double[] Mu3, double[,] Sigma1, double[,] Sigma2, double[,] Sigma3, double[] Weights)
        {
            double result = 0.0, Temp = 0.0;
            if (ComponentNumber == 0)
            {
                Temp = Weights[0] * MultivariateNormalGaussian(TestVector, Mu1, Sigma1);
                result += Temp;
                if (BTrain2)
                    result += (Weights[1] * MultivariateNormalGaussian(TestVector, Mu2, Sigma2));
                if (BTrain3)
                    result += (Weights[2] * MultivariateNormalGaussian(TestVector, Mu3, Sigma3));
            }
            else if (ComponentNumber == 1)
            {
                Temp = Weights[1] * MultivariateNormalGaussian(TestVector, Mu2, Sigma2);
                if (BTrain1)
                    result += (Weights[0] * MultivariateNormalGaussian(TestVector, Mu1, Sigma1));
                result += Temp;
                if (BTrain3)
                    result += (Weights[2] * MultivariateNormalGaussian(TestVector, Mu3, Sigma3));
            }
            else if (ComponentNumber == 2)
            {
                Temp = Weights[2] * MultivariateNormalGaussian(TestVector, Mu3, Sigma3);
                if (BTrain1)
                    result += (Weights[0] * MultivariateNormalGaussian(TestVector, Mu1, Sigma1));
                if (BTrain2)
                    result += (Weights[1] * MultivariateNormalGaussian(TestVector, Mu2, Sigma2));
                result += Temp;
            }
            Temp /= result;
            return Temp;
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
                if (isEqual[0] != 0 && isEqual[1] != 0 && isEqual[2] != 0)
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
