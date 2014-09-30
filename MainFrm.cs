using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Windows.Forms;
using System.Xml;
using Utilities;

namespace Screeny
{
    public partial class MainFrm : Form
    {
        private const string ClientId = "d5237df3a9ff3a3"; //Get a ID here: https://api.imgur.com/oauth2/addclient
        private readonly globalKeyboardHook _gkh = new globalKeyboardHook();
        private bool _active;
        private Rectangle _bounds;
        private Graphics _formGraphics;
        private int _initialX;
        private int _initialY;
        private bool _isDown;
        private static readonly Random Random = new Random();

        public MainFrm()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            FormBorderStyle = FormBorderStyle.None;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _gkh.HookedKeys.Add(Keys.PrintScreen);
            _gkh.HookedKeys.Add(Keys.Escape);

            _gkh.KeyDown += gkh_KeyDown;
            _gkh.KeyUp += gkh_KeyUp;
        }

        private void gkh_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.PrintScreen:
                    PrtScnBtnPressed();
                    break;
                case Keys.Escape:
                    if (_active)
                    {
                        _active = false;
                        TopMost = false;
                        Hide();
                        Opacity = 1;
                    }
                    break;
            }
            e.Handled = true;
        }

        private void gkh_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_active) return;
            _isDown = true;
            _initialX = e.X;
            _initialY = e.Y;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_active || !_isDown) return;
            Refresh();
            var drwaPen = new Pen(Color.Navy, 3);
            _bounds = new Rectangle(Math.Min(e.X, _initialX), Math.Min(e.Y, _initialY), Math.Abs(e.X - _initialX),
                Math.Abs(e.Y - _initialY));
            _formGraphics = CreateGraphics();
            _formGraphics.DrawRectangle(drwaPen, _bounds);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_active) return;

            _isDown = false;
            //bmp.Save(@"C:/Users/C453/Desktop/img.jpg", ImageFormat.Png); //Test

            TakeScreenshot();
        }

        public void PrtScnBtnPressed()
        {
            _active = true;
            Show();
            TopMost = true;
            WindowState = FormWindowState.Maximized;

            this.TopMost = true;

            Opacity = .5;
        }

        public void TakeScreenshot()
        {
            if (_bounds.Width == 0 || _bounds.Height == 0) return;

            var bmp = new Bitmap(_bounds.Width, _bounds.Height, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(bmp);
            _active = false;
            TopMost = false;
            Hide();
            Opacity = 1;
            g.CopyFromScreen(_bounds.Left, _bounds.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            using (var w = new WebClient())
            {
                w.Headers.Add("Authorization", "Client-ID " + ClientId);
                var values = new NameValueCollection
                {
                    {"image", Convert.ToBase64String(ImageToByte(bmp))}
                };

                var response = w.UploadValues("https://api.imgur.com/3/upload.xml", values);

                var xml = new XmlDocument();
                xml.Load(new MemoryStream(response));
                if (xml.DocumentElement == null)
                {
                    MessageBox.Show(@"There was an error uploading.");
                    return;
                };

                var selectSingleNode = xml.DocumentElement.SelectSingleNode("/data/link");
                if (selectSingleNode == null) return;

                var link = selectSingleNode.InnerText;

                var lv = new LinkViewer { Link = link };
                lv.Show();
            }
        }

        public static byte[] ImageToByte(Image img)
        {
            var converter = new ImageConverter();
            return (byte[]) converter.ConvertTo(img, typeof (byte[]));
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PrtScnBtnPressed();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            PrtScnBtnPressed();
        }
    }
}