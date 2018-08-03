using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Dofus_Updater
{
    public partial class FormSelection : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        private int shiftCount = 0;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        public FormSelection()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        private void FormSelection_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void pictureBoxFermer_MouseHover(object sender, EventArgs e)
        {
            pictureBoxFermer.Image = Properties.Resources.selection_fermer_over;
        }

        private void pictureBoxFermer_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxFermer.Image = Properties.Resources.selection_fermer;
        }

        private void pictureBoxFermer_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBoxThemeNoir_MouseHover(object sender, EventArgs e)
        {
            pictureBoxThemeNoir.Image = Properties.Resources.theme_noir_hover1;
        }

        private void pictureBoxThemeNoir_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxThemeNoir.Image = Properties.Resources.theme_noir1;
        }

        private void pictureBoxThemeBlanc_MouseHover(object sender, EventArgs e)
        {
            pictureBoxThemeBlanc.Image = Properties.Resources.theme_blanc_hover;
        }

        private void pictureBoxThemeBlanc_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxThemeBlanc.Image = Properties.Resources.theme_blanc;
        }

        private void pictureBoxThemeBlanc_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Le thème en blanc sera ajouté dans une autre mise à jour du launcher !", "Thème non disponible !");
        }

        private void FormSelection_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift)
            {
                shiftCount++;
                if (shiftCount == 3)
                {
                    textBoxMaj.Visible = true;
                    buttonValider.Visible = true;
                    textBoxMaj.Text = FormUpdater.ReadFromJsonFile<int>(Directory.GetCurrentDirectory().ToString() + "/data.json").ToString();
                }
            }
        }

        private void buttonValider_Click(object sender, EventArgs e)
        {
            FormUpdater.WriteToJsonFile<int>(Directory.GetCurrentDirectory().ToString() + "/data.json", int.Parse(textBoxMaj.Text));
            textBoxMaj.Visible = false;
            buttonValider.Visible = false;
            shiftCount = 0;
        }
    }
}
