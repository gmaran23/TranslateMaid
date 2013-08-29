using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TranslateMaidNS
{
    public partial class TranslateForm : Form
    {
        #region public members

        public string SourceText { get; set; } 

        #endregion

        #region TranslateForm constructors

        public TranslateForm()
        {
            InterceptMouse.isHitTestCalled = false;
            if (InterceptMouse._hookID == null)
            {
                InterceptMouse._hookID = InterceptMouse.SetHook(InterceptMouse._proc);
            }
            InitializeComponent();
            InitializeTimers();
        }

        public TranslateForm(string sourceText)
        {
            InterceptMouse.isHitTestCalled = false;
            if (InterceptMouse._hookID == null)
            {
                InterceptMouse._hookID = InterceptMouse.SetHook(InterceptMouse._proc);
            }

            InitializeComponent();
            InitializeTimers();

            InitializeWebRequest(sourceText);
        } 

        #endregion

        #region initializers and event handlers

        private void InitializeWebRequest(string sourceText)
        {
            string downloadUri = @"http://www.microsofttranslator.com/defaultPrev.aspx?ref=ie8activity";

            webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
            sourceText = sourceText.Replace('#', ' ').Replace('&', ' ');

            //choose between POST or GET,
            if (sourceText.Length > 700)
            {
                sourceText = sourceText.Replace("<", "< ");//.Replace('>', ' ');
                string textToTranslate = "SourceText=" + Uri.EscapeUriString(sourceText);
                byte[] postParameters = Encoding.UTF8.GetBytes(textToTranslate);
                webBrowser1.Navigate(Uri.EscapeUriString(downloadUri), "_self", postParameters, "Content-Type: application/x-www-form-urlencoded");
            }
            else
            {
                downloadUri += "&SourceText=";
                downloadUri += sourceText;
                webBrowser1.Navigate(Uri.EscapeUriString(downloadUri));
            }
        }

        private void TranslateForm_Load(object sender, EventArgs e)
        {
            IWin32Window wndHelper = this;
            int exStyle = (int)WindowStyle.GetWindowLong(wndHelper.Handle, (int)WindowStyle.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WindowStyle.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WindowStyle.SetWindowLong(wndHelper.Handle, (int)WindowStyle.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        private void TranslateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Dispose();
            timer2.Dispose();
            progressBar1.Dispose();
            webBrowser1.Dispose();
            InterceptMouse.UnhookWindowsHookEx(InterceptMouse._hookID);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.progressBar1.Hide();
            this.timer1.Stop();
        }

        private void webBrowser1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    break;
                default:
                    break;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.progressBar1.Increment(5);
            if (this.progressBar1.Value > 70)
                this.progressBar1.Value = 0;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (InterceptMouse.isHitTestCalled)
            {
                if (this != null && !this.IsDisposed)
                {
                    Point mouseLocation = new Point(InterceptMouse.pointX, InterceptMouse.pointY);
                    Point frm1Location = this.Location;

                    if (!this.DesktopBounds.Contains(mouseLocation))
                    {
                        this.Close();
                        timer2.Stop();
                    }
                }
            }
        } 

        #endregion
    }
}
