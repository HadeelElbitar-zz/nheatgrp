using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GraduationProject
{
    public partial class MainForm : Form
    {
        private int childFormNumber = 0;
        private int FrameIndix = 0;
        List<FrameInfo> Frames;
        VideoFunctions VFn = new VideoFunctions();
        FrameFunctions FFn = new FrameFunctions();

        public MainForm()
        {
            InitializeComponent();
        }

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
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            string FileName = "";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                FileName = openFileDialog.FileName;
            }
            Frames = VFn.LoadVideoFrames(FileName);
            FFn.DisplayFrame(Frames[FrameIndix] , FBox);
            FrameNumberLBL.Text += Frames.Count.ToString();
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

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void NextFrameBTN_Click(object sender, EventArgs e)
        {
            FFn.DisplayFrame(Frames[++FrameIndix], FBox);
            if (FrameIndix + 1 >= Frames.Count)
                NextFrameBTN.Visible = false;
            PreviousFrameBTN.Visible = true;
            FrameNumberLBL.Text = (FrameIndix + 1).ToString() + " / " + Frames.Count;
        }

        private void PreviousFrameBTN_Click(object sender, EventArgs e)
        {
            FFn.DisplayFrame(Frames[--FrameIndix], FBox);
            if (FrameIndix - 1 < 0)
                PreviousFrameBTN.Visible = false;
            NextFrameBTN.Visible = true;
            FrameNumberLBL.Text = (FrameIndix + 1).ToString() + " / " + Frames.Count;
        }

        private void FrameNumTBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                FrameIndix = int.Parse(FrameNumTBox.Text);
                if (FrameIndix > Frames.Count || FrameIndix < 0)
                    MessageBox.Show("Invalid Frame Number !");
                else
                {
                    FrameIndix--;
                    FFn.DisplayFrame(Frames[FrameIndix], FBox);
                    FrameNumberLBL.Text = (FrameIndix + 1).ToString() + " / " + Frames.Count;
                }
            }
            catch 
            {
                FrameIndix = 0;
                FFn.DisplayFrame(Frames[FrameIndix], FBox);
                FrameNumberLBL.Text = (FrameIndix + 1).ToString() + " / " + Frames.Count;
            }
        }
    }
}
