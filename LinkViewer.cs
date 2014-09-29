using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Screeny
{
    public partial class LinkViewer : Form
    {
        public string Link;

        public LinkViewer()
        {
            InitializeComponent();
        }

        private void LinkViewer_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            linkTextBox.Text = Link;
            PlaceLowerRight();
            this.TopMost = true;
        }


        private void PlaceLowerRight()
        {
            //Determine "rightmost" screen
            Screen rightmost = Screen.PrimaryScreen;

            this.Left = rightmost.WorkingArea.Right - this.Width;
            this.Top = rightmost.WorkingArea.Bottom - this.Height;
        }

        private void openBtn_Click(object sender, EventArgs e)
        {
            Process.Start(Link);
            this.Close();
        }

        private void copyBtn_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Link);
            this.Close();
        }
    }
}
