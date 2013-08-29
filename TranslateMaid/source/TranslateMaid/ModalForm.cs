using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TranslateMaidNS
{
    public partial class ModalForm : Form
    {
        #region ModalForm members

        private TranslateForm translateForm;
        private bool isTranslateFormClosing = default(bool);
        private bool isTranslationFormActivatedByParent = default(bool);
        private bool formShown = default(bool);
        public string SourceText { get; set; } 

        #endregion

        #region ModalForm constructor

        public ModalForm(string sourceText)
        {
            InitializeComponent();

            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = false;
            this.BackColor = Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Opacity = 0.5;
            this.translateForm = new TranslateForm(sourceText);
            this.LostFocus += new EventHandler(ModalForm_LostFocus);
            this.GotFocus += new EventHandler(ModalForm_GotFocus);
            this.Activated += new EventHandler(ModalForm_Activated);

            this.translateForm.FormClosing += new FormClosingEventHandler(TranslateForm_FormClosing);
        } 

        #endregion

        #region TranslateForm event handler

        private void TranslateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isTranslateFormClosing)
                this.Close();
        } 

        #endregion

        #region ModalForm event handler

        private void ModalForm_Activated(object sender, EventArgs e)
        {
            if (formShown)
            {
                if (!translateForm.ContainsFocus)
                {
                    isTranslationFormActivatedByParent = true;
                    translateForm.Activate();
                }
            }
        }

        private void ModalForm_GotFocus(object sender, EventArgs e)
        {
            if (formShown)
            {
                translateForm.Show();
            }
        }

        private void ModalForm_LostFocus(object sender, EventArgs e)
        {
            if (formShown)
            {
                if (!isTranslationFormActivatedByParent)
                    this.Close();
            }
        }

        private void ModalForm_Load(object sender, EventArgs e)
        {
            //IWin32Window wndHelper = this;
            //int exStyle = (int)WindowStyle.GetWindowLong(wndHelper.Handle, (int)WindowStyle.GetWindowLongFields.GWL_EXSTYLE);
            //exStyle |= (int)WindowStyle.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            //WindowStyle.SetWindowLong(wndHelper.Handle, (int)WindowStyle.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        private void ModalForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                this.Close();
        }

        private void ModalForm_KeyDown(object sender, KeyEventArgs e)
        {
            this.Close();
        }

        private void ModalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            isTranslateFormClosing = true;
            translateForm.Close();
            translateForm.Dispose();
        }

        private void ModalForm_Shown(object sender, EventArgs e)
        {
            translateForm.Opacity = 1;
            translateForm.Show();
            formShown = true;
        }

        private void ModalForm_Deactivate(object sender, EventArgs e)
        {
            if (formShown)
            {
                if (!isTranslationFormActivatedByParent)
                    this.Close();
            }
        } 

        #endregion
    }
}
