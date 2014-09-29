﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Utilities;

namespace Screeny
{
    public partial class MainFrm : Form
    {
        private readonly globalKeyboardHook _gkh = new globalKeyboardHook();
        private bool _active;
        private Rectangle _bounds;
        private Graphics _formGraphics;
        private int _initialX;
        private int _initialY;
        private bool _isDown;

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
                    _active = true;
                    TopMost = true;
                    WindowState = FormWindowState.Maximized;
                    Opacity = .5;
                    break;
                case Keys.Escape:
                    if (_active)
                    {
                        _active = false;
                        TopMost = false;
                        WindowState = FormWindowState.Minimized;
                        Opacity = 1;
                    }
                    break;
                default:
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
            int width = e.X - _initialX, height = e.Y - _initialY;
            _bounds = new Rectangle(Math.Min(e.X, _initialX), Math.Min(e.Y, _initialY), Math.Abs(e.X - _initialX),
                Math.Abs(e.Y - _initialY));
            _formGraphics = CreateGraphics();
            _formGraphics.DrawRectangle(drwaPen, _bounds);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_active) return;

            _isDown = false;
            var bmp = new Bitmap(_bounds.Width, _bounds.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(_bounds.Left, _bounds.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            //bmp.Save(@"C:/Users/C453/Desktop/img.jpg", ImageFormat.Png); //Test

            using (var w = new WebClient())
            {
                var clientID = "d5237df3a9ff3a3";
                w.Headers.Add("Authorization", "Client-ID " + clientID);
                var values = new NameValueCollection
                {
                    {"image", Convert.ToBase64String(ImageToByte(bmp))}
                };

                var response = w.UploadValues("https://api.imgur.com/3/upload.xml", values);

                var xml = new XmlDocument();
                xml.Load(new MemoryStream(response));
                var link = xml.DocumentElement.SelectSingleNode("/data/link").InnerText;

                _active = false;
                TopMost = false;
                WindowState = FormWindowState.Minimized;
                Opacity = 1;

                var lv= new LinkViewer {Link = link};
                lv.Show();
            }
        }

        public static byte[] ImageToByte(Image img)
        {
            var converter = new ImageConverter();
            return (byte[]) converter.ConvertTo(img, typeof (byte[]));
        }
    }
}