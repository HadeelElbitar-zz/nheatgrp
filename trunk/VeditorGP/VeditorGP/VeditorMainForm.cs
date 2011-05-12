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
        VideoFunctions VideoFunctionsObject;
        ContourFunctions ContourFunctionsObject;
        bool EnableSegmentation = false;
        int FrameState;
        List<CvPoint> ContourPositions;
        Frame CurrentDisplayedFrame;
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
        void OpenVideo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.avi)|*.avi|All Files (*.*)|*.*";
            string FileName = "";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                FileName = openFileDialog.FileName;
            VideoFunctionsObject = new VideoFunctions();
            CurrentDisplayedFrame = VideoFunctionsObject.OpenVideo(FileName);
            FrameBox.Image = CurrentDisplayedFrame.BmpImage;
        }
        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenVideo();
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
        }
        void CutObject()
        {
            //try
            //{
                FrameState = 0;
                ContourFunctionsObject = new ContourFunctions();
                Bitmap Temp = (Bitmap)FrameBox.Image;
                VideoFunctionsObject.InitialSegmentationBinaryFrame = ContourFunctionsObject.GetBlackAndWhiteContour(ContourPositions.ToArray(), Temp);
                VideoFunctionsObject.ConnectedContour = ContourFunctionsObject.GetConnectedContour(VideoFunctionsObject.InitialSegmentationBinaryFrame);
                VideoFunctionsObject.SetInitialWindowsArroundContour();
                //VideoFunctionsObject.TrainClassifiers();
            //}
            //catch
            //{
            //    MessageBox.Show("Please Import a video first!");
            //}
        }
        #endregion  
    }
}
