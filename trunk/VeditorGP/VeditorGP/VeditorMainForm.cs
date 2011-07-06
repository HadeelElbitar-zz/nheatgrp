using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using openCV;

namespace VeditorGP
{
    public partial class VeditorMainForm : Form
    {
        #region Variables and Constructors
        VideoFunctions VideoFunctionsObject, NewVideoFunctionsObject;
        ContourFunctions ContourFunctionsObject;
        bool EnableSegmentation = false;
        int FrameState;
        List<CvPoint> ContourPositions;
        Frame CurrentDisplayedFrame, NewCurrentDisplayedFrame;
        System.Timers.Timer MyTimer, NewTimer;
        bool PlayMood = true, NewPlayMode = true, HideStip = false;
        string FileName = "";

        public VeditorMainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Import Video
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenVideo();
        }
        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenVideo();
        }
        void OpenVideo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.avi)|*.avi|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                FileName = openFileDialog.FileName;
            VideoFunctionsObject = new VideoFunctions();
            CurrentDisplayedFrame = VideoFunctionsObject.OpenVideo(FileName);
            FrameBox.Image = CurrentDisplayedFrame.BmpImage;
        }
        #endregion

        #region Segmentation
        private void SegmentationBTN_Click(object sender, EventArgs e)
        {
            try
            {
                EnableSegmentation = true;
                ContourPositions = new List<CvPoint>();
                FrameState = 0;
                VideoFunctionsObject.InitialFrame = new Frame();
                VideoFunctionsObject.InitialFrame.BmpImage = (Bitmap)FrameBox.Image;
                VideoFunctionsObject.InitialFrame.InitializeFrame((Bitmap)FrameBox.Image);
            }
            catch
            {
                MessageBox.Show("Please Import a video first!");
            }
        }
        private void FrameBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (EnableSegmentation)
            {
                if (FrameState == 0)
                {
                    ContourPositions.Clear();
                    FrameState = 1;
                }
                ContourPositions.Add(new CvPoint(e.X, e.Y));
                int count = ContourPositions.Count;
                if (count != 1)
                {
                    Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Red);
                    Graphics formGraphics = FrameBox.CreateGraphics();
                    formGraphics.DrawLine(myPen, ContourPositions[count - 2].x, ContourPositions[count - 2].y, e.X, e.Y);
                    myPen.Dispose();
                    formGraphics.Dispose();
                }
            }
        }
        #endregion

        #region Start Cutting Object
        private void CutObjectBTN_Click(object sender, EventArgs e)
        {
            CutObject();
          //  LoadNewVideo();
            //MessageBox.Show("Finished!");
        }
        void CutObject()
        {
            try
            {
                FrameState = 0;
                ContourFunctionsObject = new ContourFunctions();
                Bitmap Temp = (Bitmap)FrameBox.Image;
                VideoFunctionsObject.InitialSegmentationBinaryFrame = ContourFunctionsObject.GetBlackAndWhiteContour(ContourPositions.ToArray(), Temp);
                VideoFunctionsObject.ConnectedContour = ContourFunctionsObject.GetConnectedContour(VideoFunctionsObject.InitialSegmentationBinaryFrame, VideoFunctionsObject.InitialContourFrame = new Frame());
                ContourFunctionsObject.GetUpperAndLowerContour(ref VideoFunctionsObject.Upper, ref VideoFunctionsObject.Lower);
                VideoFunctionsObject.SetInitialWindowsArroundContour();
                if (ShowWindowCheckBox.Checked)
                    VideoFunctionsObject.ShowWin(FrameBox);
               // VideoFunctionsObject.TrainClassifiers();
               // VideoFunctionsObject.PropagateFrame(ShowWindowCheckBox.Checked, FrameBox);
            }
            catch { }
        }
        void LoadNewVideo()
        {
            NewVideoFunctionsObject = new VideoFunctions();
            NewCurrentDisplayedFrame = NewVideoFunctionsObject.OpenVideo(VideoFunctionsObject.NewVideoPath);
            FBox.Image = NewCurrentDisplayedFrame.BmpImage;
        }
        #endregion

        #region Dr.Mostafa View Frames
        int Start = 0;
        private void VideoTabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (Start == 0 && VideoFunctionsObject != null)
            {
                DisplayNewFrames();
                Start++;
                DrMostafaNextBTN.Enabled = true;
            }
            else if (Start == 0)
                DrMostafaNextBTN.Enabled = false;
        }
        private void DrMostafaNextBTN_Click(object sender, EventArgs e)
        {
            DisplayNewFrames();
        }
        void DisplayNewFrames()
        {
            int Success = 0;
            DrMostafaPicBox1.Image = VideoFunctionsObject.GetNewFrame(ref Success);
            if (Success == -1)
            {
                DrMostafaPicBox1.Image = null;
                DrMostafaPicBox2.Image = null;
                DrMostafaPicBox3.Image = null;
                DrMostafaPicBox4.Image = null;
                DrMostafaPicBox5.Image = null;
                DrMostafaPicBox6.Image = null;
                DrMostafaNextBTN.Enabled = false;
            }
            DrMostafaPicBox2.Image = VideoFunctionsObject.GetNewFrame(ref Success);
            if (Success == -1)
            {
                DrMostafaPicBox2.Image = null;
                DrMostafaPicBox3.Image = null;
                DrMostafaPicBox4.Image = null;
                DrMostafaPicBox5.Image = null;
                DrMostafaPicBox6.Image = null;
                DrMostafaNextBTN.Enabled = false;
            }
            DrMostafaPicBox3.Image = VideoFunctionsObject.GetNewFrame(ref Success);
            if (Success == -1)
            {
                DrMostafaPicBox3.Image = null;
                DrMostafaPicBox4.Image = null;
                DrMostafaPicBox5.Image = null;
                DrMostafaPicBox6.Image = null;
                DrMostafaNextBTN.Enabled = false;
            }
            DrMostafaPicBox4.Image = VideoFunctionsObject.GetNewFrame(ref Success);
            if (Success == -1)
            {
                DrMostafaPicBox4.Image = null;
                DrMostafaPicBox5.Image = null;
                DrMostafaPicBox6.Image = null;
                DrMostafaNextBTN.Enabled = false;
            }
            DrMostafaPicBox5.Image = VideoFunctionsObject.GetNewFrame(ref Success);
            if (Success == -1)
            {
                DrMostafaPicBox5.Image = null;
                DrMostafaPicBox6.Image = null;
                DrMostafaNextBTN.Enabled = false;
            }
            DrMostafaPicBox6.Image = VideoFunctionsObject.GetNewFrame(ref Success);
            if (Success == -1)
            {
                DrMostafaPicBox6.Image = null;
                DrMostafaNextBTN.Enabled = false;
            }
        }
        #endregion

        #region Play Video Controls
        private void PlayBTN_Click(object sender, EventArgs e)
        {
            if (PlayMood) // pause 
            {
                MyTimer = new System.Timers.Timer();
                MyTimer.Elapsed += new System.Timers.ElapsedEventHandler(MyTimer_Elapsed);
                MyTimer.Interval = VideoFunctionsObject.FramePerSecond;
                MyTimer.Enabled = true;
            }
            else // stop 
            {
                VideoFunctionsObject = new VideoFunctions();
                CurrentDisplayedFrame = VideoFunctionsObject.OpenVideo(FileName);
                FrameBox.Image = CurrentDisplayedFrame.BmpImage;
                MyTimer = new System.Timers.Timer();
                MyTimer.Elapsed += new System.Timers.ElapsedEventHandler(MyTimer_Elapsed);
                MyTimer.Interval = VideoFunctionsObject.FramePerSecond;
                MyTimer.Enabled = true;
                PlayMood = true;
            }
        }
        private void PlayNewBTN_Click(object sender, EventArgs e)
        {
            if (NewPlayMode) // pause 
            {
                NewTimer = new System.Timers.Timer();
                NewTimer.Elapsed += new System.Timers.ElapsedEventHandler(NewTimer_Elapsed);
                NewTimer.Interval = NewVideoFunctionsObject.FramePerSecond;
                NewTimer.Enabled = true;
            }
            else // stop 
            {
                NewVideoFunctionsObject = new VideoFunctions();
                NewCurrentDisplayedFrame = NewVideoFunctionsObject.OpenVideo(VideoFunctionsObject.NewVideoPath);
                FBox.Image = NewCurrentDisplayedFrame.BmpImage;
                NewTimer = new System.Timers.Timer();
                NewTimer.Elapsed += new System.Timers.ElapsedEventHandler(NewTimer_Elapsed);
                NewTimer.Interval = NewVideoFunctionsObject.FramePerSecond;
                NewTimer.Enabled = true;
                NewPlayMode = true;
            }
        }
        void MyTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            FrameBox.Image = VideoFunctionsObject.GetNextFrame().BmpImage;
        }
        void NewTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            FBox.Image = NewVideoFunctionsObject.GetNextFrame().BmpImage;
        }
        private void PauseBTN_Click(object sender, EventArgs e)
        {
            MyTimer.Enabled = false;
        }
        private void PauseNewBTN_Click(object sender, EventArgs e)
        {
            NewTimer.Enabled = false;
        }
        private void StopBTN_Click(object sender, EventArgs e)
        {
            MyTimer.Enabled = false;
            PlayMood = false;
            Bitmap EmptyImage = new Bitmap(FrameBox.Image.Width, FrameBox.Image.Height);
            FrameBox.Image = EmptyImage;
        }
        private void StopNewBTN_Click(object sender, EventArgs e)
        {
            NewTimer.Enabled = false;
            NewPlayMode = false;
            Bitmap EmptyImage = new Bitmap(FBox.Image.Width, FBox.Image.Height);
            FBox.Image = EmptyImage;
        }
        #endregion#

        #region About Box
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        } 
        #endregion

        #region Tools Options
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!HideStip)
            {
                toolStrip.Hide();
                HideStip = true;
            }
            else
            {
                toolStrip.Show();
                HideStip = false;
            }
        } 
        #endregion
    }
}
