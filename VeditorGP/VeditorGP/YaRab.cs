using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Accord.Math;
using System.Drawing.Imaging;

namespace VeditorGP
{
    class YaRab
    {
        #region Variables
        Window MyWindow;
        static int Counter = 0;
        public List<Point>[] DividedPoints; //1 = Foreground , 0 = Background
        int Dimension = 3, Components = 3;
        double[,] R, G, B, ComponentWeights;
        double[, , ,] SigmaArray;
        double[, ,] MeanArray;
        byte[, ,] LAB;
        bool[,] BoolTrain;
        public YaRab(Window _window)
        {
            MyWindow = _window;
            MeanArray = new double[2, Components, Dimension];
            SigmaArray = new double[2, Components, Dimension, Dimension];
            R = MyWindow.WindowFrame.doubleRedPixels;
            G = MyWindow.WindowFrame.doubleGreenPixels;
            B = MyWindow.WindowFrame.doubleBluePixels;
            LAB = MyWindow.WindowFrame.EmguLabImage.Data;
            ComponentWeights = new double[2, Components];
            BoolTrain = new bool[2, Components];
            for (int i = 0; i < Components; i++)
            {
                ComponentWeights[0, i] = 1.0 / 3.0;
                ComponentWeights[1, i] = 1.0 / 3.0;
                BoolTrain[0, i] = true;
                BoolTrain[1, i] = true;
            }
        }
        #endregion

        #region Helping Functions
        double PriorProbability(int ComponentNumber, double[] TestVector, List<double[]> Mus, List<double[,]> Sigmas, double[] Weights, int Mode)
        {
            double result = 0.0, Temp = 0.0;
            if (ComponentNumber == 0)
            {
                Temp = Weights[0] * MultivariateNormalGaussian(TestVector, Mus[0], Sigmas[0]);
                result += Temp;
                if (BoolTrain[Mode, 1])
                    result += (Weights[1] * MultivariateNormalGaussian(TestVector, Mus[1], Sigmas[1]));
                if (BoolTrain[Mode, 2])
                    result += (Weights[2] * MultivariateNormalGaussian(TestVector, Mus[2], Sigmas[2]));
            }
            else if (ComponentNumber == 1)
            {
                Temp = Weights[1] * MultivariateNormalGaussian(TestVector, Mus[1], Sigmas[1]);
                if (BoolTrain[Mode, 0])
                    result += (Weights[0] * MultivariateNormalGaussian(TestVector, Mus[0], Sigmas[0]));
                result += Temp;
                if (BoolTrain[Mode, 2])
                    result += (Weights[2] * MultivariateNormalGaussian(TestVector, Mus[2], Sigmas[2]));
            }
            else if (ComponentNumber == 2)
            {
                Temp = Weights[2] * MultivariateNormalGaussian(TestVector, Mus[2], Sigmas[2]);
                if (BoolTrain[Mode, 0])
                    result += (Weights[0] * MultivariateNormalGaussian(TestVector, Mus[0], Sigmas[0]));
                if (BoolTrain[Mode, 1])
                    result += (Weights[1] * MultivariateNormalGaussian(TestVector, Mus[1], Sigmas[1]));
                result += Temp;
            }
            Temp /= result;
            return Temp;
        }
        double[,] GetCovariance(double[] MeanMatrix, List<double[]> DataSet)
        {
            double[,] CovarianceMatrix = new double[Dimension, Dimension];
            double ReadMean = 0, GreenMean = 0, BlueMean = 0, Lmean = 0, Amean = 0, Bmean = 0;
            int length;
            length = DataSet.Count;
            for (int i = 0; i < length; i++)
            {
                ReadMean += Math.Pow((DataSet[i][0] - MeanMatrix[0]), 2);
                GreenMean += Math.Pow((DataSet[i][1] - MeanMatrix[1]), 2);
                BlueMean += Math.Pow((DataSet[i][2] - MeanMatrix[2]), 2);
                //Lmean += Math.Pow((DataSet[i][3] - MeanMatrix[3]), 2);
                //Amean += Math.Pow((DataSet[i][4] - MeanMatrix[4]), 2);
                //Bmean += Math.Pow((DataSet[i][5] - MeanMatrix[5]), 2);
                //Lmean += Math.Pow((DataSet[i][0] - MeanMatrix[0]), 2);
                //Amean += Math.Pow((DataSet[i][1] - MeanMatrix[1]), 2);
                //Bmean += Math.Pow((DataSet[i][2] - MeanMatrix[2]), 2);
            }
            CovarianceMatrix[0, 0] = ReadMean / length;
            CovarianceMatrix[1, 1] = GreenMean / length;
            CovarianceMatrix[2, 2] = BlueMean / length;
            //CovarianceMatrix[3, 3] = Lmean / length;
            //CovarianceMatrix[4, 4] = Amean / length;
            //CovarianceMatrix[5, 5] = Bmean / length;
            //CovarianceMatrix[0, 0] = Lmean / length;
            //CovarianceMatrix[1, 1] = Amean / length;
            //CovarianceMatrix[2, 2] = Bmean / length;
            return CovarianceMatrix;
        }
        double[] FindNewMean(List<double[]> List)
        {
            double[] Result = new double[Dimension];
            int Count = List.Count;
            foreach (double[] item in List)
                for (int i = 0; i < Dimension; i++)
                    Result[i] += item[i];
            for (int i = 0; i < Dimension; i++)
                Result[i] /= Count;
            return Result;
        }
        double MatrixDet(double[,] SubMatrix)
        {
            double res = 0;
            if (SubMatrix.Length == 4)
                res += (SubMatrix[0, 0] * SubMatrix[1, 1] - SubMatrix[0, 1] * SubMatrix[1, 0]);
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
        double MultivariateNormalGaussian(double[] X, double[] Mu, double[,] Sigma)
        {
            double Result = 0.0;
            double Term = 1.0 / Math.Pow((2.0 * Math.PI), (double)((double)Dimension / 2.0));
            double Determ = MatrixDet(Sigma);
            double power =  Math.Pow(Determ, 0.5);
            Term *= power;
            double[] Difference = new double[Dimension];
            for (int i = 0; i < Dimension; i++)
                Difference[i] = X[i] - Mu[i];
            double[,] SigmaInverse = Matrix.Inverse(Sigma);
            double[] MulRes = new double[Dimension];
            for (int i = 0; i < Dimension; i++)
                for (int j = 0; j < Dimension; j++)
                    MulRes[i] += (Difference[i] * SigmaInverse[j, i]);
            double Temp = 0.0;
            for (int i = 0; i < Dimension; i++)
                Temp += (Difference[i] * MulRes[i]);
            Temp *= -0.5;
            Result = Term * Math.Exp(Temp);
            return Result;
        }
        void DividData()
        {
            int Width = MyWindow.WindowFrame.width, Height = MyWindow.WindowFrame.height;
            DividedPoints = new List<Point>[2];
            DividedPoints[0] = new List<Point>();
            DividedPoints[1] = new List<Point>();
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    if (MyWindow.WindowBinaryMask.byteRedPixels[i, j] == 0)
                        DividedPoints[0].Add(new Point(j, i));
                    else
                        DividedPoints[1].Add(new Point(j, i));
                }
        }
        double GetDistance(double[] A, double[] B)
        {
            double Result = 0.0;
            for (int i = 0; i < Dimension; i++)
                Result += Math.Pow((A[i] - B[i]), 2);
            return Math.Sqrt(Result);
        }
        int[] TestEqualCentroids(List<double[]> centroids)
        {
            int[] Result = new int[Components];
            Result[0] = (int)GetDistance(centroids[0], centroids[1]);
            Result[1] = (int)GetDistance(centroids[0], centroids[2]);
            Result[2] = (int)GetDistance(centroids[2], centroids[1]);
            return Result;
        }
        #endregion

        #region K-means
        void FindKmean(int Mode) //0 = B , 1 = F
        {
            #region Find Centroids
            List<double[]> centroids = new List<double[]>();
            Random rand = new Random();
            int Length = DividedPoints[Mode].Count;
            for (int k = 0; k < Components; k++)
            {
                int i = rand.Next(0, Length);
                double[] temp = new double[Dimension];
                temp[0] = R[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                temp[1] = G[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                temp[2] = B[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                //temp[3] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 0];
                //temp[4] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 1];
                //temp[5] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 2];
                //temp[0] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 0];
                //temp[1] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 1];
                //temp[2] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 2];
                centroids.Add(temp);
            }
            while (true)
            {
                int[] isEqual = TestEqualCentroids(centroids);
                if (isEqual[0] != 0 && isEqual[1] != 0 && isEqual[2] != 0)
                    break;
                centroids.Clear();
                for (int k = 0; k < Components; k++)
                {
                    int i = rand.Next(0, Length);
                    double[] temp = new double[Dimension];
                    temp[0] = R[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                    temp[1] = G[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                    temp[2] = B[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                    //temp[3] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 0];
                    //temp[4] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 1];
                    //temp[5] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 2];
                    //temp[0] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 0];
                    //temp[1] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 1];
                    //temp[2] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 2];
                    centroids.Add(temp);
                }
            }
            #endregion
            #region Convergince
            bool ValueChanged = true;
            double MinValue = double.MaxValue;
            double DictanceResult;
            int SelectedIndex = -1;
            List<double[]>[] AssignedVectors = new List<double[]>[Components]; //0 , 1 , 2
            for (int i = 0; i < Components; i++)
                AssignedVectors[i] = new List<double[]>();
            while (ValueChanged)
            {
                ValueChanged = false;
                for (int i = 0; i < Components; i++)
                    AssignedVectors[i].Clear();
                for (int i = 0; i < Length; i++)
                {
                    double[] temp = new double[Dimension];
                    temp[0] = R[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                    temp[1] = G[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                    temp[2] = B[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                    //temp[3] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 0];
                    //temp[4] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 1];
                    //temp[5] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 2];
                    //temp[0] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 0];
                    //temp[1] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 1];
                    //temp[2] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 2];
                    MinValue = double.MaxValue;
                    for (int j = 0; j < Components; j++)
                    {
                        DictanceResult = GetDistance(centroids[j], temp);
                        if (DictanceResult < MinValue)
                        {
                            MinValue = DictanceResult;
                            SelectedIndex = j;
                        }
                    }
                    AssignedVectors[SelectedIndex].Add(temp);
                }
                List<double[]> TempCentroids = new List<double[]>();
                for (int i = 0; i < Components; i++)
                {
                    TempCentroids.Add(FindNewMean(AssignedVectors[i]));
                    DictanceResult = GetDistance(TempCentroids[i], centroids[i]);
                    if (DictanceResult > 0.001)
                    {
                        ValueChanged = true;
                        centroids[i] = TempCentroids[i];
                    }
                }
                if (!ValueChanged)
                    break;
            }
            #endregion
            #region Assign Means and Sigmas
            for (int i = 0; i < Components; i++)
                for (int j = 0; j < Dimension; j++)
                    MeanArray[Mode, i, j] = centroids[i][j];
            double[,] TempSigma = new double[Dimension, Dimension];
            double[] TempMenan = new double[Dimension];
            for (int i = 0; i < Components; i++)
            {
                for (int j = 0; j < Dimension; j++)
                    TempMenan[j] = MeanArray[Mode, i, j];
                TempSigma = GetCovariance(TempMenan, AssignedVectors[i]);
                for (int c = 0; c < Dimension; c++)
                    for (int k = 0; k < Dimension; k++)
                        SigmaArray[Mode, i, c, k] = TempSigma[c, k];
            }
            #endregion
        }
        #endregion

        #region Expectation Maximization (EM)
        void CalculateEM(int Mode)
        {
            #region Initializations
            double[] TempComponentWeights, NewWeights = new double[Components];
            double Priors;
            double[] PriorSummation = new double[Components];
            List<double[]> TempMeans = new List<double[]>();
            List<double[,]> TempSigmas = new List<double[,]>();
            List<double[]> Means = new List<double[]>();
            List<double[,]> Sigmas = new List<double[,]>();
            TempMeans.Add(new double[Dimension]);
            TempMeans.Add(new double[Dimension]);
            TempMeans.Add(new double[Dimension]);
            TempSigmas.Add(new double[Dimension, Dimension]);
            TempSigmas.Add(new double[Dimension, Dimension]);
            TempSigmas.Add(new double[Dimension, Dimension]);
            int Length = DividedPoints[Mode].Count;
            for (int i = 0; i < Components; i++)
            {
                NewWeights[i] = ComponentWeights[Mode, i];
                double[] TempM = new double[Dimension];
                double[,] TempS = new double[Dimension, Dimension];
                for (int j = 0; j < Dimension; j++)
                    TempM[j] = MeanArray[Mode, i, j];
                Means.Add(TempM);
                for (int c = 0; c < Dimension; c++)
                    for (int k = 0; k < Dimension; k++)
                        TempS[c, k] = SigmaArray[Mode, i, c, k];
                Sigmas.Add(TempS);
            }
            #endregion
            #region Iterations
            while ((GetDistance(TempMeans[0], Means[0]) > 0.01 && ComponentWeights[Mode, 0] != 0) || (GetDistance(TempMeans[1], Means[1]) > 0.01 && ComponentWeights[Mode, 1] != 0) || (GetDistance(TempMeans[2], Means[2]) > 0.01 && ComponentWeights[Mode, 2] != 0))
            {
                PriorSummation = new double[Components];
                TempComponentWeights = new double[Components];
                NewWeights.CopyTo(TempComponentWeights, 0);
                #region Check Sigma and Copy Values
                for (int i = 0; i < Components; i++)
                    for (int c = 0; c < Dimension; c++)
                        if (Sigmas[i][c, c] == 0 || double.IsNaN(Sigmas[i][c, c]))
                        {
                            BoolTrain[Mode, i] = false;
                            ComponentWeights[Mode, i] = 0;
                            NewWeights[i] = 0;
                            TempComponentWeights[i] = 0;
                            break;
                        }
                for (int i = 0; i < Components; i++)
                    if (BoolTrain[Mode, i])
                    {
                        Means[i].CopyTo(TempMeans[i], 0);
                        Means[i] = new double[Dimension];
                        for (int k = 0; k < Dimension; k++)
                            TempSigmas[i][k, k] = Sigmas[i][k, k];
                        Sigmas[i] = new double[Dimension, Dimension];
                    }
                #endregion
                for (int i = 0; i < Components; i++)
                {
                    if (BoolTrain[Mode, i])
                    {
                        #region Means
                        for (int j = 0; j < Length; j++)
                        {
                            double[] temp = new double[Dimension];
                            temp[0] = R[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                            temp[1] = G[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                            temp[2] = B[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                            //temp[3] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 0];
                            //temp[4] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 1];
                            //temp[5] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 2];
                            //temp[0] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 0];
                            //temp[1] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 1];
                            //temp[2] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 2];
                            Priors = PriorProbability(i, temp, TempMeans, TempSigmas, TempComponentWeights, Mode);
                            PriorSummation[i] += Priors;
                            for (int c = 0; c < Dimension; c++)
                                Means[i][c] += (Priors * temp[c]);
                        }
                        NewWeights[i] = PriorSummation[i] / Length;
                        for (int c = 0; c < Dimension; c++)
                            Means[i][c] /= PriorSummation[i];
                        #endregion
                        #region Sigmas
                        for (int j = 0; j < Length; j++)
                        {
                            double[] temp = new double[Dimension];
                            temp[0] = R[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                            temp[1] = G[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                            temp[2] = B[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X];
                            //temp[3] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 0];
                            //temp[4] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 1];
                            //temp[5] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 2];
                            //temp[0] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 0];
                            //temp[1] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 1];
                            //temp[2] = LAB[DividedPoints[Mode][i].Y, DividedPoints[Mode][i].X, 2];
                            Priors = PriorProbability(i, temp, TempMeans, TempSigmas, TempComponentWeights, Mode);
                            for (int c = 0; c < Dimension; c++)
                                Sigmas[i][c, c] += (Priors * Math.Pow((temp[c] - Means[i][c]), 2));
                        }
                        for (int c = 0; c < Dimension; c++)
                            Sigmas[i][c, c] /= PriorSummation[i];
                        #endregion
                    }
                }
            }
            #endregion
            #region Copy Values
            for (int i = 0; i < Components; i++)
            {
                ComponentWeights[Mode, i] = NewWeights[i];
                for (int j = 0; j < Dimension; j++)
                    MeanArray[Mode, i, j] = Means[i][j];
                for (int c = 0; c < Dimension; c++)
                    for (int k = 0; k < Dimension; k++)
                        SigmaArray[Mode, i, c, k] = Sigmas[i][c, k];
            }
            #endregion
            #region Final Check
            for (int i = 0; i < Components; i++)
                for (int c = 0; c < Dimension; c++)
                    if (Sigmas[i][c, c] == 0 || double.IsNaN(Sigmas[i][c, c]))
                    {
                        BoolTrain[Mode, i] = false;
                        ComponentWeights[Mode, i] = 0;
                        break;
                    }
            #endregion
        }
        #endregion

        #region Training and Classification
        public void Train()
        {
            DividData();
            FindKmean(1);
            FindKmean(0);
            CalculateEM(1);
            CalculateEM(0);
            GMM();
            int x = 0;
        }
        public byte[,] GMM()
        {
            #region Variables
            List<double[]> Means = new List<double[]>();
            List<double[,]> Sigmas = new List<double[,]>();
            for (int l = 0; l < 2; l++)
                for (int i = 0; i < Components; i++)
                {
                    double[] TempM = new double[Dimension];
                    double[,] TempS = new double[Dimension, Dimension];
                    for (int j = 0; j < Dimension; j++)
                        TempM[j] = MeanArray[l, i, j];
                    Means.Add(TempM);
                    for (int c = 0; c < Dimension; c++)
                        for (int k = 0; k < Dimension; k++)
                            TempS[c, k] = SigmaArray[l, i, c, k];
                    Sigmas.Add(TempS);
                }
            double[] Sample;
            int width = MyWindow.WindowFrame.width, height = MyWindow.WindowFrame.height;
            byte[,] TempClassify = new byte[height, width];
            double[] ForeGroundProbability, BackGroundProbability;
            MyWindow.ForegroundProbability = new double[height, width]; 
            #endregion
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    Sample = new double[Dimension];
                    ForeGroundProbability = new double[Components];
                    BackGroundProbability = new double[Components];
                    Sample[0] = R[i, j];
                    Sample[1] = G[i, j];
                    Sample[2] = B[i, j];

                    #region EM
                    for (int c = 0; c < Components; c++)
                    {
                        if (BoolTrain[0, c])
                            BackGroundProbability[c] = (MultivariateNormalGaussian(Sample, Means[c], Sigmas[c])) * ComponentWeights[0, c];
                        if (BoolTrain[1, c])
                            ForeGroundProbability[c] = (MultivariateNormalGaussian(Sample, Means[c + 3], Sigmas[c + 3])) * ComponentWeights[1, c + 3];
                    }
                    #endregion

                    MyWindow.ForegroundProbability[i, j] = (ForeGroundProbability[0] + ForeGroundProbability[1] + ForeGroundProbability[2]) /
                        (ForeGroundProbability[0] + ForeGroundProbability[1] + ForeGroundProbability[2] +
                        BackGroundProbability[0] + BackGroundProbability[1] + BackGroundProbability[2]);
                    if (MyWindow.ForegroundProbability[i, j] < 0.5)
                        TempClassify[i, j] = 0;
                    else
                        TempClassify[i, j] = 255;
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
                        p[0] = TempClassify[i, j];
                        p[1] = TempClassify[i, j];
                        p[2] = TempClassify[i, j];
                        p += 3;
                    }
                    p += space;
                }
            }
            NewImage.UnlockBits(bmpData);
            string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Classifier " + Counter + ".bmp";
            Counter++;
            NewImage.Save(Pw, ImageFormat.Bmp);
            #endregion

            #region Fc
            #endregion

            return TempClassify;
        }
        #endregion
    }
}
