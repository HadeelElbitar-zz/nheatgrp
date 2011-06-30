using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using openCV;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;

namespace VeditorGP
{
    class Window
    {
        #region Variables
        public int Center_X, Center_Y, WindowSize;
        public Frame WindowFrame, WindowBinaryMask, WindowContour;
        public Classifier WindowClassifier;
        public double ColorConfidence;
        public double[,] ForegroundProbability, WeightingFunction, ShapeConfidence;
        public List<Point> WindowContourPoints;
        static int Counter = 0, IntegrationCounter = 0;
        double SigmaS;
        public byte[,] UpdatedBinaryFrame, ClassificationMask;
        double[,] IntegratedForegroundProbability,AveragedForegroundProbability;
        #endregion

        #region Constructor
        public Window(int width, int height, Frame ColoredFrame, Frame BinaryMask, Point CenterPoint, Frame ContourFrame)
        {
            #region Initialization of Variables
            WindowContourPoints = new List<Point>();
            WindowFrame = new Frame();
            WindowFrame.height = height;
            WindowFrame.width = width;
            if (width % 2 == 0) WindowFrame.width++;
            if (height % 2 == 0) WindowFrame.height++;

            WindowBinaryMask = new Frame();
            WindowBinaryMask.height = height;
            WindowBinaryMask.width = width;
            if (width % 2 == 0) WindowBinaryMask.width++;
            if (height % 2 == 0) WindowBinaryMask.height++;

            WindowContour = new Frame();
            WindowContour.height = height;
            WindowContour.width = width;
            if (width % 2 == 0) WindowContour.width++;
            if (height % 2 == 0) WindowContour.height++;

            WindowSize = WindowFrame.width * WindowFrame.height;
            Center_X = CenterPoint.X;
            Center_Y = CenterPoint.Y;
            #endregion

            #region Initialization of Arrays
            WindowFrame.byteRedPixels = new byte[WindowFrame.height, WindowFrame.width];
            WindowFrame.byteGreenPixels = new byte[WindowFrame.height, WindowFrame.width];
            WindowFrame.byteBluePixels = new byte[WindowFrame.height, WindowFrame.width];

            WindowBinaryMask.byteRedPixels = new byte[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.byteGreenPixels = new byte[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.byteBluePixels = new byte[WindowBinaryMask.height, WindowBinaryMask.width];

            WindowContour.byteRedPixels = new byte[WindowContour.height, WindowContour.width];
            WindowContour.byteGreenPixels = new byte[WindowContour.height, WindowContour.width];
            WindowContour.byteBluePixels = new byte[WindowContour.height, WindowContour.width];

            WindowContour.doubleRedPixels = new double[WindowFrame.height, WindowContour.width];
            WindowContour.doubleGreenPixels = new double[WindowFrame.height, WindowContour.width];
            WindowContour.doubleBluePixels = new double[WindowFrame.height, WindowContour.width];

            WindowFrame.doubleRedPixels = new double[WindowFrame.height, WindowFrame.width];
            WindowFrame.doubleGreenPixels = new double[WindowFrame.height, WindowFrame.width];
            WindowFrame.doubleBluePixels = new double[WindowFrame.height, WindowFrame.width];

            WindowBinaryMask.doubleRedPixels = new double[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.doubleGreenPixels = new double[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.doubleBluePixels = new double[WindowBinaryMask.height, WindowBinaryMask.width];
            #endregion

            int M = (WindowFrame.width - 1) / 2, N = (WindowFrame.height - 1) / 2;
            for (int i = (Center_Y - N), c = 0; i < ColoredFrame.height && i <= (Center_Y + N); i++, c++)
            {
                if (i < 0) i = 0;
                for (int j = (Center_X - M), k = 0; j < ColoredFrame.width && j <= (Center_X + M); j++, k++)
                {
                    if (j < 0) j = 0;
                    #region Fill Colored Frame
                    WindowFrame.byteRedPixels[c, k] = ColoredFrame.byteRedPixels[i, j];
                    WindowFrame.byteGreenPixels[c, k] = ColoredFrame.byteGreenPixels[i, j];
                    WindowFrame.byteBluePixels[c, k] = ColoredFrame.byteBluePixels[i, j];
                    WindowFrame.doubleRedPixels[c, k] = ColoredFrame.doubleRedPixels[i, j];
                    WindowFrame.doubleGreenPixels[c, k] = ColoredFrame.doubleGreenPixels[i, j];
                    WindowFrame.doubleBluePixels[c, k] = ColoredFrame.doubleBluePixels[i, j];
                    #endregion

                    #region Fill Binary Mask
                    WindowBinaryMask.byteRedPixels[c, k] = BinaryMask.byteRedPixels[i, j];
                    WindowBinaryMask.byteGreenPixels[c, k] = BinaryMask.byteGreenPixels[i, j];
                    WindowBinaryMask.byteBluePixels[c, k] = BinaryMask.byteBluePixels[i, j];
                    WindowBinaryMask.doubleRedPixels[c, k] = BinaryMask.doubleRedPixels[i, j];
                    WindowBinaryMask.doubleGreenPixels[c, k] = BinaryMask.doubleGreenPixels[i, j];
                    WindowBinaryMask.doubleBluePixels[c, k] = BinaryMask.doubleBluePixels[i, j];
                    #endregion

                    #region Fill Window Contour
                    WindowContour.byteRedPixels[c, k] = ContourFrame.byteRedPixels[i, j];
                    WindowContour.byteGreenPixels[c, k] = ContourFrame.byteGreenPixels[i, j];
                    WindowContour.byteBluePixels[c, k] = ContourFrame.byteBluePixels[i, j];
                    WindowContour.doubleRedPixels[c, k] = ContourFrame.doubleRedPixels[i, j];
                    WindowContour.doubleGreenPixels[c, k] = ContourFrame.doubleGreenPixels[i, j];
                    WindowContour.doubleBluePixels[c, k] = ContourFrame.doubleBluePixels[i, j];
                    if (WindowContour.byteRedPixels[c, k] == 255 || WindowContour.byteGreenPixels[c, k] == 255 || WindowContour.byteBluePixels[c, k] == 255)
                        WindowContourPoints.Add(new Point(k, c));
                    #endregion
                }
            }
            WindowFrame.InitializeWindowFrame();
            WindowBinaryMask.InitializeWindowFrame();
            WindowContour.InitializeWindowFrame();

            #region Test Saving Window Frames
            //string Nw = "Window Frame " + Counter.ToString() + ".bmp";
            ////Counter++;
            //string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + Nw;
            //WindowFrame.BmpImage.Save(Pw, ImageFormat.Bmp);
            //Nw = "Window Binary Mask " + Counter.ToString() + ".bmp";
            //Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + Nw;
            //WindowBinaryMask.BmpImage.Save(Pw, ImageFormat.Bmp);
            //Counter++;
            //Nw = "Window Contour " + Counter.ToString() + ".bmp";
            //Counter++;
            //Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + Nw;
            //WindowContour.BmpImage.Save(Pw, ImageFormat.Bmp);
            #endregion

            WindowClassifier = new Classifier(this);
        }
        #endregion

        #region Calculate models for initial frame and other frames
        public void CalculateModels()//Calculate models for initial frame
        {
            #region Initializations
            int width = WindowFrame.width, height = WindowFrame.height;
            double SigmaCSquare = 202500.0;
            WeightingFunction = new double[height, width];
            ShapeConfidence = new double[height, width];
            #endregion
            CalculateWeightFunction(height, width, SigmaCSquare);
            CalculateColorConfidence(height, width);
            CalculateSigmaS(height, width);
            CalculateInitialShapeConfidence(height, width, SigmaCSquare);
            CalculateWindowForegroundProbability();
        }
        public void CalculateModels(Window HistoryWindow)//Calculate models for other frames
        {
            #region Initializations
            double SigmaCSquare = 202500.0;
            int width = WindowFrame.width, height = WindowFrame.height;
            WeightingFunction = new double[height, width];
            ShapeConfidence = new double[height, width];
            #endregion

            #region Count foreground points in History and Updated Franes
            int HistoryForegroundPoints = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    if (WindowBinaryMask.byteBluePixels[i, j] == 255)
                        HistoryForegroundPoints++;
            int UpdatedForegroundPoints = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    if (UpdatedBinaryFrame[i, j] == 255)
                        UpdatedForegroundPoints++; 
            #endregion

            #region Use history or updated frame
            if ((UpdatedForegroundPoints - HistoryForegroundPoints) <= 80)//if PcU has more foreground use McH, otherwise use McU
            {
                CalculateWeightFunction(height, width, SigmaCSquare);
                CalculateColorConfidence(height, width);// If Updated Color Model is used, update the color confidence
                CalculateSigmaS(height, width);
                CalculateShapeConfidence(height, width, SigmaCSquare);
            }
            else //Use McH
            {
                ColorConfidence = HistoryWindow.ColorConfidence;
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                    {
                        ForegroundProbability[i, j] = HistoryWindow.ForegroundProbability[i, j];
                        WeightingFunction[i, j] = HistoryWindow.WeightingFunction[i, j];
                        ShapeConfidence[i, j] = HistoryWindow.ShapeConfidence[i, j];
                    }
            }
            #endregion
            CalculateWindowForegroundProbability();
        }
        #endregion

        #region Shape and Color Confidence Calculations
        void CalculateWeightFunction(int height, int width, double SigmaCSquare)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double Distance = double.MaxValue;
                    foreach (Point item in WindowContourPoints)
                    {
                        double Temp = Math.Sqrt(Math.Pow((item.X - i), 2) + Math.Pow((item.Y - j), 2));
                        if (Temp < Distance)
                            Distance = Temp;
                    }
                    double NegativeDistanceSquare = (-1 * Math.Pow(Distance, 2));
                    WeightingFunction[i, j] = Math.Exp(NegativeDistanceSquare / SigmaCSquare);
                }
            }
        }
        void CalculateInitialShapeConfidence(int height, int width, double SigmaCSquare)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double Distance = double.MaxValue;
                    foreach (Point item in WindowContourPoints)
                    {
                        double Temp = Math.Sqrt(Math.Pow((item.X - i), 2) + Math.Pow((item.Y - j), 2));
                        if (Temp < Distance)
                            Distance = Temp;
                    }
                    double NegativeDistanceSquare = (-1 * Math.Pow(Distance, 2));
                    ShapeConfidence[i, j] = 1 - Math.Exp(NegativeDistanceSquare / Math.Pow(SigmaS, 2));
                    //WeightingFunction[i, j] = Math.Exp(NegativeDistanceSquare / SigmaCSquare);
                }
            }
        }
        void CalculateShapeConfidence(int height, int width, double SigmaCSquare)
        {
            double FGThreshold = 0.75, BGThreshold = 0.25;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double Distance = double.MaxValue;
                    foreach (Point item in WindowContourPoints)
                    {
                        double Temp = Math.Sqrt(Math.Pow((item.X - i), 2) + Math.Pow((item.Y - j), 2));
                        if (Temp < Distance)
                            Distance = Temp;
                    }
                    double NegativeDistanceSquare = (-1 * Math.Pow(Distance, 2));
                    ShapeConfidence[i, j] = 1 - Math.Exp(NegativeDistanceSquare / (Math.Pow(SigmaS, 2)));
                    if (ShapeConfidence[i, j] > FGThreshold) WindowClassifier.ForegroundPoints.Add(new Point(i, j));
                    else if (ShapeConfidence[i, j] < BGThreshold) WindowClassifier.BackgroundPoints.Add(new Point(i, j));
                }
            }
        }
        void CalculateColorConfidence(int height, int width)
        {
            double Summation = 0.0, WeightsSummation = 0.0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int SegmentationLabel = 0;
                    if (WindowBinaryMask.byteRedPixels[i, j] == 255)
                        SegmentationLabel = 1;
                    Summation += (Math.Abs(SegmentationLabel - ForegroundProbability[i, j]) * WeightingFunction[i, j]);
                    WeightsSummation += WeightingFunction[i, j];
                }
            }
            ColorConfidence = 1 - (Summation / WeightsSummation);
        }
        void CalculateSigmaS(int height, int width)
        {
            double CutOff = 0.85, r = 2;
            double SigmaMin = 2, SigmaMax = height * width;
            SigmaS = SigmaMin;
            if (ColorConfidence > CutOff && ColorConfidence <= 1)
            {
                double a = (SigmaMax - SigmaMin) / (Math.Pow((1 - CutOff), r));
                SigmaS = SigmaMin + (a * Math.Pow(ColorConfidence - CutOff, r));
            }
        }
        #endregion

        #region Update Window Centers
        public Window(Point NewCenter, Frame NewFrame, Window HistoryObject, Image<Gray, Single> FlowX, Image<Gray, Single> FlowY)
        {
            #region Initialization
            WindowContourPoints = new List<Point>();
            int XIndex, YIndex;
            Center_X = NewCenter.X;
            Center_Y = NewCenter.Y;
            WindowClassifier = HistoryObject.WindowClassifier;
            WindowFrame = new Frame();
            WindowFrame.height = 30;
            WindowFrame.width = 30;
            if ((30 % 2) == 0) WindowFrame.width++;
            if ((30 % 2) == 0) WindowFrame.height++;

            WindowBinaryMask = new Frame();
            WindowBinaryMask.height = WindowFrame.height;
            WindowBinaryMask.width = WindowFrame.width;
            if (WindowFrame.width % 2 == 0) WindowBinaryMask.width++;
            if (WindowFrame.height % 2 == 0) WindowBinaryMask.height++;

            WindowContour = new Frame();
            WindowContour.height = WindowFrame.height;
            WindowContour.width = WindowFrame.width;
            if (WindowFrame.width % 2 == 0) WindowContour.width++;
            if (WindowFrame.height % 2 == 0) WindowContour.height++;

            WindowSize = WindowFrame.width * WindowFrame.height;

            WindowFrame.byteRedPixels = new byte[WindowFrame.height, WindowFrame.width];
            WindowFrame.byteGreenPixels = new byte[WindowFrame.height, WindowFrame.width];
            WindowFrame.byteBluePixels = new byte[WindowFrame.height, WindowFrame.width];

            WindowFrame.doubleRedPixels = new double[WindowFrame.height, WindowFrame.width];
            WindowFrame.doubleGreenPixels = new double[WindowFrame.height, WindowFrame.width];
            WindowFrame.doubleBluePixels = new double[WindowFrame.height, WindowFrame.width];

            WindowBinaryMask.byteRedPixels = new byte[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.byteGreenPixels = new byte[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.byteBluePixels = new byte[WindowBinaryMask.height, WindowBinaryMask.width];

            WindowBinaryMask.doubleRedPixels = new double[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.doubleGreenPixels = new double[WindowBinaryMask.height, WindowBinaryMask.width];
            WindowBinaryMask.doubleBluePixels = new double[WindowBinaryMask.height, WindowBinaryMask.width];

            WindowContour.byteRedPixels = new byte[WindowContour.height, WindowContour.width];
            WindowContour.byteGreenPixels = new byte[WindowContour.height, WindowContour.width];
            WindowContour.byteBluePixels = new byte[WindowContour.height, WindowContour.width];

            WindowContour.doubleRedPixels = new double[WindowFrame.height, WindowContour.width];
            WindowContour.doubleGreenPixels = new double[WindowFrame.height, WindowContour.width];
            WindowContour.doubleBluePixels = new double[WindowFrame.height, WindowContour.width];
            #endregion

            #region Fill Binary Mask and Window Contour
            for (int i = 0; i < 31; i++)
                for (int j = 0; j < 31; j++)
                {
                    XIndex = i + (int)(float.Parse(FlowX.Data[i, j, 0].ToString()));
                    YIndex = j + (int)(float.Parse(FlowY.Data[i, j, 0].ToString()));
                    #region Fill Binary Mask
                    WindowBinaryMask.byteRedPixels[XIndex, YIndex] = HistoryObject.WindowBinaryMask.byteRedPixels[i, j];
                    WindowBinaryMask.byteGreenPixels[XIndex, YIndex] = HistoryObject.WindowBinaryMask.byteGreenPixels[i, j];
                    WindowBinaryMask.byteBluePixels[XIndex, YIndex] = HistoryObject.WindowBinaryMask.byteBluePixels[i, j];
                    WindowBinaryMask.doubleRedPixels[XIndex, YIndex] = HistoryObject.WindowBinaryMask.doubleRedPixels[i, j];
                    WindowBinaryMask.doubleGreenPixels[XIndex, YIndex] = HistoryObject.WindowBinaryMask.doubleGreenPixels[i, j];
                    WindowBinaryMask.doubleBluePixels[XIndex, YIndex] = HistoryObject.WindowBinaryMask.doubleBluePixels[i, j];
                    #endregion

                    #region Fill Window Contour
                    WindowContour.byteRedPixels[XIndex, YIndex] = HistoryObject.WindowContour.byteRedPixels[i, j];
                    WindowContour.byteGreenPixels[XIndex, YIndex] = HistoryObject.WindowContour.byteGreenPixels[i, j];
                    WindowContour.byteBluePixels[XIndex, YIndex] = HistoryObject.WindowContour.byteBluePixels[i, j];
                    WindowContour.doubleRedPixels[XIndex, YIndex] = HistoryObject.WindowContour.doubleRedPixels[i, j];
                    WindowContour.doubleGreenPixels[XIndex, YIndex] = HistoryObject.WindowContour.doubleGreenPixels[i, j];
                    WindowContour.doubleBluePixels[XIndex, YIndex] = HistoryObject.WindowContour.doubleBluePixels[i, j];
                    if (WindowContour.byteRedPixels[XIndex, YIndex] == 255 || WindowContour.byteGreenPixels[XIndex, YIndex] == 255 || WindowContour.byteBluePixels[XIndex, YIndex] == 255)
                        WindowContourPoints.Add(new Point(j, i));
                    #endregion
                }
            #endregion

            #region Fill Frame Data
            int M = (WindowFrame.width - 1) / 2, N = (WindowFrame.height - 1) / 2;
            for (int i = (Center_Y - N), c = 0; i < NewFrame.height && i <= (Center_Y + N); i++, c++)
            {
                if (i < 0) i = 0;
                for (int j = (Center_X - M), k = 0; j < NewFrame.width && j <= (Center_X + M); j++, k++)
                {
                    if (j < 0) j = 0;
                    #region Fill Colored Frame
                    WindowFrame.byteRedPixels[c, k] = NewFrame.byteRedPixels[i, j];
                    WindowFrame.byteGreenPixels[c, k] = NewFrame.byteGreenPixels[i, j];
                    WindowFrame.byteBluePixels[c, k] = NewFrame.byteBluePixels[i, j];
                    WindowFrame.doubleRedPixels[c, k] = NewFrame.doubleRedPixels[i, j];
                    WindowFrame.doubleGreenPixels[c, k] = NewFrame.doubleGreenPixels[i, j];
                    WindowFrame.doubleBluePixels[c, k] = NewFrame.doubleBluePixels[i, j];
                    #endregion
                }
            }
            WindowFrame.InitializeWindowFrame();
            WindowBinaryMask.InitializeWindowFrame();
            WindowContour.InitializeWindowFrame();
            #endregion

            #region Test Saving Window Frames
            //string Nw = "Window Frame " + Counter.ToString() + ".bmp";
            //Counter++;
            //string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + Nw;
            //WindowFrame.BmpImage.Save(Pw, ImageFormat.Bmp);
            #endregion
        }
        #endregion

        #region Integrated Shap and Color Foreground Probability
        //The following function isn't used yet, but it implements section 2.4 -> Equation (5)..
        void CalculateWindowForegroundProbability()
        {
            //Foreground Probability of Point x in window W
            //Window Foreground Probability =  fs(x) * L t+1(x) + (1- fs(x)* Pc(x))
            //The point may exist in overlapped windows
            int width = WindowFrame.width, height = WindowFrame.height;
            byte[,] TempClassify = new byte[height, width];
            IntegratedForegroundProbability = new double[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    int SegmentationLabel = 0;
                    if (WindowBinaryMask.byteRedPixels[i, j] == 255 || WindowBinaryMask.byteBluePixels[i, j] == 255 || WindowBinaryMask.byteGreenPixels[i, j] == 255)
                        SegmentationLabel = 1;
                    IntegratedForegroundProbability[i, j] = (ShapeConfidence[i, j] * SegmentationLabel) + ((1 - ShapeConfidence[i, j]) * ForegroundProbability[i, j]);
                    if (IntegratedForegroundProbability[i, j] < 0.5)
                        TempClassify[i, j] = 0;
                    else
                        TempClassify[i, j] = 255;
                }
            ClassificationMask = TempClassify;
            #region Test Saving Classified Image
            Bitmap NewImage = new Bitmap(width, height);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    NewImage.SetPixel(j, i, Color.FromArgb(TempClassify[i, j], TempClassify[i, j], TempClassify[i, j]));
            string Pw = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Integrated Classifier" + IntegrationCounter + ".bmp";
            IntegrationCounter++;
            NewImage.Save(Pw, ImageFormat.Bmp);
            #endregion
        } 
        #endregion
    }
}