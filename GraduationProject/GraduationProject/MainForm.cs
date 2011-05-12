using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using openCV;
using Emgu.CV.UI;
using System.Runtime.InteropServices;

namespace GraduationProject
{
    public partial class MainForm : Form
    {
        #region Variables
        private int childFormNumber = 0;
        private int FrameIndix = 0;
        Frame Frame;
        VideoFunctions VFn = new VideoFunctions();
        FrameFunctions FFn= new FrameFunctions();
        ContourFunctions CFn = new ContourFunctions(30,30);
        List<CvPoint> ContourPositions;
        int FrameState;

        public MainForm()
        {
            InitializeComponent();
        }
        #endregion
        Frame _Frame;
        Frame newPictureItem;
        #region Menus
        private void ShowNewForm(object sender, EventArgs e)
        {
            Form childForm = new Form();
            childForm.MdiParent = this;
            childForm.Text = "Window " + childFormNumber++;
            childForm.Show();
        }
        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.avi)|*.avi|All Files (*.*)|*.*";
            string FileName = "";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                FileName = openFileDialog.FileName;
            }
            Frame = VFn.LoadVideoFrames(FileName);
            FFn.DisplayFrame(Frame, FBox);
            FrameNumberLBL.Text = (FrameIndix + 1).ToString();
        }
        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;
            }
        }
        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }
        #endregion

        #region MDI Related
        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }
        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }
        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }
        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }
        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }
        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }
        #endregion

        #region FormLoad
        private void MainForm_Load(object sender, EventArgs e)
        {
            Frame = new Frame();
            ContourPositions = new List<CvPoint>();
            FrameState = 0;
        }
        #endregion

        #region Surf Frames
        private void NextFrameBTN_Click(object sender, EventArgs e)
        {
            try
            {
                _Frame = VFn.GetNextFrame();
                FFn.DisplayFrame(_Frame, FBox);
                FrameIndix++;
                SIFT S = new SIFT();
                ContourFunctions CFn = new ContourFunctions(_Frame.width, _Frame.height);
                FrameNumberLBL.Text = (FrameIndix + 1).ToString();
                // FBox.Image = S.GetSIFTpoints(VideoFunctions.Frames[0], VideoFunctions.Frames[1]);
               // FBox.Image = CFn.GetContour(_Frame).BmpImage;
               // FBox.Image = CFn.GetContour(_Frame).BmpImage;
            }
            catch { }
        }
        private void PreviousFrameBTN_Click(object sender, EventArgs e)
        {
            FFn.DisplayFrame(VideoFunctions.Frames[FrameIndix - 1], FBox);
            FrameNumberLBL.Text = (FrameIndix - 1).ToString();
        }
        #endregion

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
            //try
            //{
            string[] PicturePath = new string[20];
            OpenFileDialog Picture = new OpenFileDialog();
            Picture.Filter = "All Files (*.*)|*.*";
            Picture.Multiselect = true;
            if (Picture.ShowDialog() == DialogResult.OK)
                PicturePath = Picture.FileNames;
            int count = PicturePath.Count();
            for (int k = 0; k < count; k++)
            {
                newPictureItem = new Frame();
                //Frame Result ;//= new Frame();
                string PictureName = PicturePath[k].Substring(PicturePath[k].LastIndexOf('\\') + 1);
                int offset = PictureName.LastIndexOf('.') + 1;
                string type = PictureName.Substring(offset, PictureName.Length - offset);
                newPictureItem.OpenFrame(PicturePath[k], FBox);
                //newPictureItem.
                //Result = CFn.GetContour(newPictureItem);
                FFn.DisplayFrame(newPictureItem, FBox);
                //SIFT S = new SIFT();
                //FBox.Image = S.GetSIFTpoints(Result, newPictureItem);
            }
            //}
            //catch { }
        }
        private void FBox_MouseClick(object sender, MouseEventArgs e)
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
                Graphics formGraphics = FBox.CreateGraphics();
                formGraphics.DrawLine(myPen, ContourPositions[count - 2].x, ContourPositions[count - 2].y, e.X, e.Y);
                myPen.Dispose();
                formGraphics.Dispose();
            }
        }
        private void FinishContourButton_Click(object sender, EventArgs e)
        {
            FrameState = 0;
            Frame frame = CFn.GetBlackAndWhiteContour(ContourPositions.ToArray(), (Bitmap)FBox.Image);
            Frame Result = CFn.GetContour(frame, newPictureItem);
            //FFn.DisplayFrame(newPictureItem, FBox);
            FBox.Image = (Image)frame.BmpImage;
        }
    }
}