using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Math;

namespace EMTester
{
    class EmClass
    {
        public EmClass(byte[,] Red, byte[,] Green, byte[,] Blue, int ImageWidth, int ImageHeight)
        {
            RedPixels = Red;
            GreenPixels = Green;
            BluePixels = Blue;
            width = ImageWidth;
            height = ImageHeight;
            Size = width * height;
            OurForegroundEM();
        }
        #region Variables and constructors
        byte[,] RedPixels, GreenPixels, BluePixels;
        int width, height, Size;
        bool FTrain1 = true, FTrain2 = true, FTrain3 = true, FTrain4 = true;
        double[] FWeights;
        static double[,] ForegroundEMCovariance1, ForegroundEMCovariance2, ForegroundEMCovariance3, ForegroundEMCovariance4;
        static double[] ForegroundEMmean1, ForegroundEMmean2, ForegroundEMmean3, ForegroundEMmean4;
        #endregion

        #region Training Step
        void OurForegroundEM()
        {
            #region Initialization and Variables
            double[] OldMu1, OldMu2, OldMu3, OldMu4, TempWeights;
            double[,] OldSigma1, OldSigma2, OldSigma3, OldSigma4;
            OldMu1 = new double[3];
            OldMu2 = new double[3];
            OldMu3 = new double[3];
            OldMu4 = new double[3];
            OldSigma1 = new double[3, 3];
            OldSigma2 = new double[3, 3];
            OldSigma3 = new double[3, 3];
            OldSigma4 = new double[3, 3];
            //ForegroundEMCovariance1 = {{1.0,0.0,0.0},{0.0,1.0,0.0},{0.0,0.0,1.0}};
            //ForegroundEMCovariance2 = {{1.0,0.0,0.0},{0.0,1.0,0.0},{0.0,0.0,1.0}};
            //ForegroundEMCovariance3 = {{1.0,0.0,0.0},{0.0,1.0,0.0},{0.0,0.0,1.0}};
            ForegroundEMCovariance1 = new double[3, 3];
            ForegroundEMCovariance1[0, 0] = 1000.0;
            ForegroundEMCovariance1[1, 1] = 1000.0;
            ForegroundEMCovariance1[2, 2] = 1000.0;
            ForegroundEMCovariance2 = new double[3, 3];
            ForegroundEMCovariance2[0, 0] = 1000.0;
            ForegroundEMCovariance2[1, 1] = 1000.0;
            ForegroundEMCovariance2[2, 2] = 1000.0;
            ForegroundEMCovariance3 = new double[3, 3];
            ForegroundEMCovariance3[0, 0] = 1000.0;
            ForegroundEMCovariance3[1, 1] = 1000.0;
            ForegroundEMCovariance3[2, 2] = 1000.0;
            ForegroundEMCovariance4 = new double[3, 3];
            ForegroundEMCovariance4[0, 0] = 1000.0;
            ForegroundEMCovariance4[1, 1] = 1000.0;
            ForegroundEMCovariance4[2, 2] = 1000.0;
            FWeights = new double[4];
            FWeights[0] = 1.0 / 4.0;
            FWeights[1] = 1.0 / 4.0;
            FWeights[2] = 1.0 / 4.0;
            FWeights[3] = 1.0 / 4.0;
            double SummationComponent1 = 0.0, SummationComponent2 = 0.0, SummationComponent3 = 0.0, SummationComponent4 = 0.0;
            double Prior1, Prior2, Prior3, Prior4;
            double[] Sample;
            //ForegroundEMmean1 = {1.0,1.0,1.0};
            //ForegroundEMmean2 = {1.0,1.0,1.0};
            //ForegroundEMmean3 = {1.0,1.0,1.0};
            ForegroundEMmean1 = new double[3];
            ForegroundEMmean1[0] = 50.0;
            ForegroundEMmean1[1] = 100.0;
            ForegroundEMmean1[2] = 250.0;
            ForegroundEMmean2 = new double[3];
            ForegroundEMmean2[0] = 250.0;
            ForegroundEMmean2[1] = 100.0;
            ForegroundEMmean2[2] = 170.0;
            ForegroundEMmean3 = new double[3];
            ForegroundEMmean3[0] = 30.0;
            ForegroundEMmean3[1] = 200.0;
            ForegroundEMmean3[2] = 90.0;
            ForegroundEMmean4 = new double[3];
            ForegroundEMmean4[0] = 230.0;
            ForegroundEMmean4[1] = 230.0;
            ForegroundEMmean4[2] = 60.0;
            #endregion

            #region iterations
            while ((Distance(OldMu1, ForegroundEMmean1) > 0.01 && FWeights[0] != 0)
                || (Distance(OldMu2, ForegroundEMmean2) > 0.01 && FWeights[1] != 0)
                || (Distance(OldMu3, ForegroundEMmean3) > 0.01 && FWeights[2] != 0)
                || (Distance(OldMu4, ForegroundEMmean4) > 0.01 && FWeights[3] != 0))
            {
                TempWeights = new double[4];
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
                if (ForegroundEMCovariance4[0, 0] == 0 || double.IsNaN(ForegroundEMCovariance4[0, 0]) || ForegroundEMCovariance4[1, 1] == 0 || ForegroundEMCovariance4[2, 2] == 0)
                {
                    FTrain4 = false;
                    FWeights[3] = 0;
                }
                else
                {
                    OldMu4 = new double[3];
                    OldSigma4 = new double[3, 3];
                    ForegroundEMmean4.CopyTo(OldMu4, 0);
                    OldSigma4 = ForegroundEMCovariance4;
                    ForegroundEMCovariance4 = new double[3, 3];
                    ForegroundEMmean4 = new double[3];
                    SummationComponent4 = 0.0;
                }
                #endregion
                #region Train First Component
                if (FTrain1)
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Sample = new double[3];
                            Sample[0] = RedPixels[i, j];
                            Sample[1] = GreenPixels[i, j];
                            Sample[2] = BluePixels[i, j];
                            Prior1 = PriorProbability(0, Sample, OldMu1, OldMu2, OldMu3, OldMu4, OldSigma1, OldSigma2, OldSigma3, OldSigma4, TempWeights);
                            if (double.IsNaN(Prior1))
                                Prior1 = 0.00001;
                            SummationComponent1 += Prior1;
                            ForegroundEMmean1[0] += (Prior1 * Sample[0]);
                            ForegroundEMmean1[1] += (Prior1 * Sample[1]);
                            ForegroundEMmean1[2] += (Prior1 * Sample[2]);
                        }
                    }
                    FWeights[0] = SummationComponent1 / Size;
                    ForegroundEMmean1[0] /= SummationComponent1;
                    ForegroundEMmean1[1] /= SummationComponent1;
                    ForegroundEMmean1[2] /= SummationComponent1;
                    #region Sigma
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Sample = new double[3];
                            Sample[0] = RedPixels[i, j];
                            Sample[1] = GreenPixels[i, j];
                            Sample[2] = BluePixels[i, j];
                            Prior1 = PriorProbability(0, Sample, OldMu1, OldMu2, OldMu3, OldMu4, OldSigma1, OldSigma2, OldSigma3, OldSigma4, TempWeights);
                            ForegroundEMCovariance1[0, 0] += (Prior1 * Math.Pow((Sample[0] - ForegroundEMmean1[0]), 2));
                            ForegroundEMCovariance1[1, 1] += (Prior1 * Math.Pow((Sample[1] - ForegroundEMmean1[1]), 2));
                            ForegroundEMCovariance1[2, 2] += (Prior1 * Math.Pow((Sample[2] - ForegroundEMmean1[2]), 2));
                        }
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
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Sample = new double[3];
                            Sample[0] = RedPixels[i, j];
                            Sample[1] = GreenPixels[i, j];
                            Sample[2] = BluePixels[i, j];
                            Prior2 = PriorProbability(1, Sample, OldMu1, OldMu2, OldMu3, OldMu4, OldSigma1, OldSigma2, OldSigma3, OldSigma4, TempWeights);
                            SummationComponent2 += Prior2;
                            ForegroundEMmean2[0] += (Prior2 * Sample[0]);
                            ForegroundEMmean2[1] += (Prior2 * Sample[1]);
                            ForegroundEMmean2[2] += (Prior2 * Sample[2]);
                        }
                    }
                    FWeights[1] = SummationComponent2 / Size;
                    ForegroundEMmean2[0] /= SummationComponent2;
                    ForegroundEMmean2[1] /= SummationComponent2;
                    ForegroundEMmean2[2] /= SummationComponent2;
                    #region Sigma
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Sample = new double[3];
                            Sample[0] = RedPixels[i, j];
                            Sample[1] = GreenPixels[i, j];
                            Sample[2] = BluePixels[i, j];
                            Prior2 = PriorProbability(1, Sample, OldMu1, OldMu2, OldMu3, OldMu4, OldSigma1, OldSigma2, OldSigma3, OldSigma4, TempWeights);
                            ForegroundEMCovariance2[0, 0] += (Prior2 * Math.Pow((Sample[0] - ForegroundEMmean2[0]), 2));
                            ForegroundEMCovariance2[1, 1] += (Prior2 * Math.Pow((Sample[1] - ForegroundEMmean2[1]), 2));
                            ForegroundEMCovariance2[2, 2] += (Prior2 * Math.Pow((Sample[2] - ForegroundEMmean2[2]), 2));
                        }
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
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Sample = new double[3];
                            Sample[0] = RedPixels[i, j];
                            Sample[1] = GreenPixels[i, j];
                            Sample[2] = BluePixels[i, j];
                            Prior3 = PriorProbability(2, Sample, OldMu1, OldMu2, OldMu3, OldMu4, OldSigma1, OldSigma2, OldSigma3, OldSigma4, TempWeights);
                            SummationComponent3 += Prior3;
                            ForegroundEMmean3[0] += (Prior3 * Sample[0]);
                            ForegroundEMmean3[1] += (Prior3 * Sample[1]);
                            ForegroundEMmean3[2] += (Prior3 * Sample[2]);
                        }
                    }
                    FWeights[2] = SummationComponent3 / Size;
                    ForegroundEMmean3[0] /= SummationComponent3;
                    ForegroundEMmean3[1] /= SummationComponent3;
                    ForegroundEMmean3[2] /= SummationComponent3;
                    #region Sigma
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Sample = new double[3];
                            Sample[0] = RedPixels[i, j];
                            Sample[1] = GreenPixels[i, j];
                            Sample[2] = BluePixels[i, j];
                            Prior3 = PriorProbability(2, Sample, OldMu1, OldMu2, OldMu3, OldMu4, OldSigma1, OldSigma2, OldSigma3, OldSigma4, TempWeights);
                            ForegroundEMCovariance3[0, 0] += (Prior3 * Math.Pow((Sample[0] - ForegroundEMmean3[0]), 2));
                            ForegroundEMCovariance3[1, 1] += (Prior3 * Math.Pow((Sample[1] - ForegroundEMmean3[1]), 2));
                            ForegroundEMCovariance3[2, 2] += (Prior3 * Math.Pow((Sample[2] - ForegroundEMmean3[2]), 2));
                        }
                    }
                    ForegroundEMCovariance3[0, 0] /= SummationComponent3;
                    ForegroundEMCovariance3[1, 1] /= SummationComponent3;
                    ForegroundEMCovariance3[2, 2] /= SummationComponent3;
                    #endregion
                }
                #endregion
                #region Fourth Third Component
                if (FTrain4)
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Sample = new double[3];
                            Sample[0] = RedPixels[i, j];
                            Sample[1] = GreenPixels[i, j];
                            Sample[2] = BluePixels[i, j];
                            Prior4 = PriorProbability(3, Sample, OldMu1, OldMu2, OldMu3, OldMu4, OldSigma1, OldSigma2, OldSigma3, OldSigma4, TempWeights);
                            SummationComponent4 += Prior4;
                            ForegroundEMmean4[0] += (Prior4 * Sample[0]);
                            ForegroundEMmean4[1] += (Prior4 * Sample[1]);
                            ForegroundEMmean4[2] += (Prior4 * Sample[2]);
                        }
                    }
                    FWeights[3] = SummationComponent4 / Size;
                    ForegroundEMmean4[0] /= SummationComponent4;
                    ForegroundEMmean4[1] /= SummationComponent4;
                    ForegroundEMmean4[2] /= SummationComponent4;
                    #region Sigma
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Sample = new double[3];
                            Sample[0] = RedPixels[i, j];
                            Sample[1] = GreenPixels[i, j];
                            Sample[2] = BluePixels[i, j];
                            Prior4 = PriorProbability(3, Sample, OldMu1, OldMu2, OldMu3, OldMu4, OldSigma1, OldSigma2, OldSigma3, OldSigma4, TempWeights);
                            ForegroundEMCovariance4[0, 0] += (Prior4 * Math.Pow((Sample[0] - ForegroundEMmean4[0]), 2));
                            ForegroundEMCovariance4[1, 1] += (Prior4 * Math.Pow((Sample[1] - ForegroundEMmean4[1]), 2));
                            ForegroundEMCovariance4[2, 2] += (Prior4 * Math.Pow((Sample[2] - ForegroundEMmean4[2]), 2));
                        }
                    }
                    ForegroundEMCovariance4[0, 0] /= SummationComponent4;
                    ForegroundEMCovariance4[1, 1] /= SummationComponent4;
                    ForegroundEMCovariance4[2, 2] /= SummationComponent4;
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Final Check!
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
            if (ForegroundEMCovariance4[0, 0] == 0 || double.IsNaN(ForegroundEMCovariance4[0, 0]) || ForegroundEMCovariance4[1, 1] == 0 || ForegroundEMCovariance4[2, 2] == 0)
            {
                FTrain4 = false;
                FWeights[3] = 0;
            }
            #endregion
        }
        double MultivariateNormalGaussian(double[] X, double[] Mu, double[,] Sigma)
        {
            double Result = 0.0;
            double Term = 1 / Math.Pow((2 * Math.PI), 3 / 2);
            double Determ = MatrixDet(Sigma);
            //if (Determ == 0)
            //    Determ = 0.0000001;
            Term *= Math.Pow(Determ, 1 / 2);
            double[] Difference = new double[3];
            for (int i = 0; i < 3; i++)
                Difference[i] = X[i] - Mu[i];
            double[,] SigmaInverse = Matrix.Inverse(Sigma);
            //if(double.IsNaN(SigmaInverse[0,0] ))
            //    SigmaInverse[0, 0] = 0.0000001;
            //if (double.IsNaN(SigmaInverse[1, 1]))
            //    SigmaInverse[1, 1] = 0.0000001;
            //if (double.IsNaN(SigmaInverse[2, 2]))
            //    SigmaInverse[2, 2] = 0.0000001;
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
        double Distance(double[] A, double[] B)
        {
            double Result = 0.0;
            Result = Math.Sqrt(Math.Pow(A[0] - B[0], 2) + Math.Pow(A[1] - B[1], 2) + Math.Pow(A[2] - B[2], 2));
            return Result;
        }
        double PriorProbability(int ComponentNumber, double[] TestVector, double[] Mu1, double[] Mu2, double[] Mu3, double[] Mu4,
            double[,] Sigma1, double[,] Sigma2, double[,] Sigma3, double[,] Sigma4, double[] Weights)
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
                if (FTrain4)
                    result += (Weights[3] * MultivariateNormalGaussian(TestVector, Mu4, Sigma4));
            }
            else if (ComponentNumber == 1)
            {
                Temp = Weights[1] * MultivariateNormalGaussian(TestVector, Mu2, Sigma2);
                if (FTrain1)
                    result += (Weights[0] * MultivariateNormalGaussian(TestVector, Mu1, Sigma1));
                result += Temp;
                if (FTrain3)
                    result += (Weights[2] * MultivariateNormalGaussian(TestVector, Mu3, Sigma3));
                if (FTrain4)
                    result += (Weights[3] * MultivariateNormalGaussian(TestVector, Mu4, Sigma4));
            }
            else if (ComponentNumber == 2)
            {
                Temp = Weights[2] * MultivariateNormalGaussian(TestVector, Mu3, Sigma3);
                if (FTrain1)
                    result += (Weights[0] * MultivariateNormalGaussian(TestVector, Mu1, Sigma1));
                if (FTrain2)
                    result += (Weights[1] * MultivariateNormalGaussian(TestVector, Mu2, Sigma2));
                if (FTrain4)
                    result += (Weights[3] * MultivariateNormalGaussian(TestVector, Mu4, Sigma4));
                result += Temp;
            }
            else if (ComponentNumber == 3)
            {
                Temp = Weights[3] * MultivariateNormalGaussian(TestVector, Mu4, Sigma4);
                if (FTrain1)
                    result += (Weights[0] * MultivariateNormalGaussian(TestVector, Mu1, Sigma1));
                if (FTrain2)
                    result += (Weights[1] * MultivariateNormalGaussian(TestVector, Mu2, Sigma2));
                if (FTrain3)
                    result += (Weights[2] * MultivariateNormalGaussian(TestVector, Mu3, Sigma3));
                result += Temp;
            }
            if (result == 0)
                return 0;
            Temp /= result;
            return Temp;
        }
        #endregion
    }
}
