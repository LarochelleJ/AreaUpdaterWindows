using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace Dofus_Updater
{
    public partial class FormUpdater : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private List<Image> imagesNews = new List<Image>();
        private List<PictureBox> pagination = new List<PictureBox>();
        private Form formSelection = new FormSelection();
        private Constants c = new Constants();
        private bool themeNoir = true;
        // Liens
        private string siteWeb = "https://area-serveur.eu";
        private string forum = "https://area-serveur.eu/forum";
        private string voter = "https://area-serveur.eu/?page=voter";
        private string updaterRepo = "http://d1kcos6xihuznd.cloudfront.net/newUpdater";
        // Barre de progression
        private int percent = 0;
        private bool telechargementFini = false;
        private bool extractionFini = false;
        // Workers
        BackgroundWorker worker = new BackgroundWorker();
        // Serial values
        private int versionActuelle = 0;
        // Task Timer
        private System.Timers.Timer taskTimer = new System.Timers.Timer();

        private int indexImage = 1;
        public int Index
        {
            get { return indexImage; }
            set { updateIndex(value); }
        }

        public int Percent
        {
            get { return percent; }
            set
            {
                if (value < 0) { percent = 0; }
                else
                if (value > 100) { percent = 100; }
                else
                {
                    percent = value;
                }
                renderProgressBar();
            }
        }

        public FormUpdater()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.Text = "Updater Area";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Lime;
            this.TransparencyKey = Color.Lime;
            pictureBoxBG.SendToBack();
            pictureBoxPrev.Parent = pictureBoxBG;
            pictureBoxNext.Parent = pictureBoxBG;
            pictureBoxPag1.Parent = pictureBoxBG;
            pictureBoxPag2.Parent = pictureBoxBG;
            pictureBoxPag3.Parent = pictureBoxBG;
            pictureBoxPag4.Parent = pictureBoxBG;
            pictureBoxPag5.Parent = pictureBoxBG;
            pictureBoxSite.Parent = pictureBoxBG;
            pictureBoxForum.Parent = pictureBoxBG;
            pictureBoxVoter.Parent = pictureBoxBG;
            pictureBoxPB.Parent = pictureBoxBG;
            pictureBoxJouer.Visible = false;
            pictureBoxPB.SendToBack();
            pictureBoxPB.Visible = false;

            imagesNews.Add(Properties.Resources.news_5);
            pagination.Add(pictureBoxPag1);
            imagesNews.Add(Properties.Resources.news_2);
            pagination.Add(pictureBoxPag2);
            imagesNews.Add(Properties.Resources.news_3);
            pagination.Add(pictureBoxPag3);
            imagesNews.Add(Properties.Resources.news_4);
            pagination.Add(pictureBoxPag4);
            imagesNews.Add(Properties.Resources.news_1);
            pagination.Add(pictureBoxPag5);

            // On sélectionne aléatoirement une image de news
            Index = new Random().Next(0, 5);

        }

        public void start()
        {
            taskTimer.Elapsed += new ElapsedEventHandler(verifUpdate);
            taskTimer.Interval = 60000; // 1 minute      
            label.Text = "";
            if (!File.Exists(Directory.GetCurrentDirectory().ToString() + "/Dofus.exe")) // Dofus n'est pas installé
            {
                DialogResult dialogResult = MessageBox.Show("Dofus ne semble pas être installé dans ce répertoire, souhaitez-vous installer Dofus ?", "Avertissement", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    label.Text = "Téléchargement de Dofus 1.29...";
                    download(updaterRepo + "/Dofus.zip", "dg.zip");
                    label.Text = "Installation de Dofus 1.29...";
                    unzip("dg.zip");
                    label.Text = "Installation de Dofus 1.29 terminé !";
                    if (File.Exists(Directory.GetCurrentDirectory().ToString() + "/dg.zip"))
                    {
                        File.Delete(Directory.GetCurrentDirectory().ToString() + "/dg.zip");
                    }
                    try
                    {
                        Thread.Sleep(500);
                    }
                    catch { }
                    start();
                }
                else if (dialogResult == DialogResult.No)
                {
                    label.Text = "Dofus ne semble pas être installé, avez-vous installé le launcher dans le bon répertoire ?";
                }
            }
            else // Si Dofus est installé
            {
                string nomApp = System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToString().ToLower();
                if (nomApp == "updater") // role : updater
                {
                    label.Text = "Téléchargement du fichier de mise à jour...";
                    download(updaterRepo + "/Dofus-Updater.exe", "Dofus-Updater.exe");
                    Process.Start(Directory.GetCurrentDirectory().ToString() + "/Dofus-Updater.exe");
                    Application.Exit();
                }
                else // role : launcher
                {
                    if (nomApp != "dofus-updater")
                    {
                        MessageBox.Show("Il est conseillé de ne pas changer le nom du launcher, renommez le launcher en Dofus-Updater.exe", "Avertissement");
                        Application.Exit();
                    }
                    else if (Process.GetProcessesByName("Dofus-Updater").Length > 1)
                    {
                        MessageBox.Show("Le launcher est déjà ouvert !", "Avertissement");
                        Application.Exit();
                    }
                    else
                    {
                        bool justUpdated = File.Exists(Directory.GetCurrentDirectory().ToString() + "/updater.exe");
                        string currentMD5 = "";
                        string md5FromFile = "";
                        if (!justUpdated) // Ne vient pas juste d'update
                        {
                            label.Text = "Vérifications des mises à jour du launcher...";
                            download(updaterRepo + "/Dofus-Updater.exe", "updater.exe");
                            currentMD5 = md5(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                            md5FromFile = md5(Directory.GetCurrentDirectory().ToString() + "/updater.exe");
                        }
                        if (justUpdated || currentMD5.Equals(md5FromFile)) // À jour, on peut vérifier les majs des fichiers du jeu
                        {
                            if (update(false))
                            {
                                // Notification task bar, mises à jour terminées
                                if (this.WindowState == FormWindowState.Minimized)
                                {
                                    notifyIcon.BalloonTipText = "Le launcher a installé toutes les mises à jour du jeu !";
                                    notifyIcon.Visible = true;
                                    notifyIcon.ShowBalloonTip(500);
                                }
                            }
                            taskTimer.Enabled = true; // activé la vérification auto des mises à jour
                        }
                        else // role : update du launcher
                        {
                            label.Text = "Mise à jour de votre launcher...";

                            try
                            {
                                Thread.Sleep(1000);
                            }
                            catch { }
                            Process.Start(Directory.GetCurrentDirectory().ToString() + "/updater.exe");
                            Application.Exit();
                        }
                    }
                }
            }
        }

        private void verifUpdate(object source, ElapsedEventArgs e)
        {
            if (update(true))
            {
                notifyIcon.BalloonTipText = "Le launcher a installé des mises à jour, il serait conseillé de relancer votre jeu !";
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(500);
            }
        }

        private bool update(bool backgroundUpdate)
        {
            bool madeAnUpdate = false;
            pictureBoxJouer.Visible = false;
            if (File.Exists(Directory.GetCurrentDirectory().ToString() + "/updater.exe"))
            {
                File.Delete(Directory.GetCurrentDirectory().ToString() + "/updater.exe");
            }
            label.Text = "Votre launcher est à jour !";
            versionActuelle = ReadFromJsonFile<int>(Directory.GetCurrentDirectory().ToString() + "/data.json");
            bool totallyUpdated = false;
            while (!totallyUpdated)
            {
                if (versionActuelle == 1) // optimisation des fichiers de jeu, on regroupe dans dans la première version (version 0) pour accélérer l'installation
                {
                    versionActuelle = ReadFromJsonFile<int>(Directory.GetCurrentDirectory().ToString() + "/data.json");
                }
                string nomFichierMaj = (versionActuelle + 1).ToString() + ".zip";
                if (!backgroundUpdate)
                {
                    label.Text = "[Version " + (versionActuelle + 1) + "] Téléchargement des fichiers de la mise à jour du jeu...";
                }
                download(updaterRepo + "/" + nomFichierMaj, nomFichierMaj);
                if (new FileInfo(Directory.GetCurrentDirectory().ToString() + "/" + nomFichierMaj).Length == 0)
                {
                    totallyUpdated = true;
                }
                else // Mise à jour à faire
                {
                    madeAnUpdate = true;
                    pictureBoxJouer.Visible = false;
                    label.Text = "[Version " + (versionActuelle + 1) + "] Décompression des fichiers de la mise à jour...";
                    unzip(nomFichierMaj);
                    versionActuelle++;
                }
                if (File.Exists(Directory.GetCurrentDirectory().ToString() + "/" + nomFichierMaj))
                {
                    File.Delete(Directory.GetCurrentDirectory().ToString() + "/" + nomFichierMaj);
                }
            }

            WriteToJsonFile<int>(Directory.GetCurrentDirectory().ToString() + "/data.json", versionActuelle);
            pictureBoxPB.Visible = false;
            label.Visible = false;
            pictureBoxJouer.Visible = true;
            return madeAnUpdate;
        }

        private string md5(string cheminFichier)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(cheminFichier))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty).ToLower();
                }
            }
        }

        private void renderPictureBoxBG(Image news)
        {
            Bitmap finalImage = new Bitmap(756, 516);
            Bitmap imageNews = new Bitmap(news);
            Bitmap bg = new Bitmap(c._BG);
            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(imageNews, new Rectangle(17, 55, imageNews.Width, imageNews.Height));
                // Le bg va par dessus l'image de news
                g.DrawImage(bg, new Rectangle(0, 0, bg.Width, bg.Height));
            }
            pictureBoxBG.Image = finalImage;
        }

        private void renderProgressBar()
        {
            Bitmap finalImage = new Bitmap(662, 18);
            Bitmap imagePb = new Bitmap(Properties.Resources.pb);
            Bitmap bg = new Bitmap(Properties.Resources.pb_bg);
            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(bg, new Rectangle(0, 0, bg.Width, bg.Height));
                g.DrawImage(imagePb, new Rectangle(0, 0, imagePb.Width * percent / 100, imagePb.Height));
            }
            pictureBoxPB.Image = finalImage;
        }

        private void unzip(string nomFichierMaj)
        {
            pictureBoxPB.Visible = true;

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (o, e) =>
            {
                Percent = e.ProgressPercentage;
            };
            worker.DoWork += (o, e) =>
            {
                using (ZipFile zip = ZipFile.Read(Directory.GetCurrentDirectory().ToString() + "/" + nomFichierMaj))
                {
                    double coefficient = 1 / (double)zip.Count * 100.00;
                    double percentageFinal = 0;
                    foreach (ZipEntry file in zip)
                    {
                        file.Extract(Directory.GetCurrentDirectory().ToString(), ExtractExistingFileAction.OverwriteSilently);
                        percentageFinal += coefficient;
                        worker.ReportProgress((int)percentageFinal);
                    }
                    extractionFini = true;
                }
            };

            worker.RunWorkerAsync();

            while (!extractionFini)
            {
                Application.DoEvents();
            }
            extractionFini = false;
            Percent = 0;
            pictureBoxPB.Visible = false;
        }

        private void download(string url, string nomFichier)
        {
            pictureBoxPB.Visible = true;
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new System.Uri(url),
                Directory.GetCurrentDirectory().ToString() + "/" + nomFichier);
            }
            while (!telechargementFini)
            {
                Application.DoEvents();
            }
            telechargementFini = false;
            pictureBoxPB.Visible = false;
            Percent = 0;
        }

        private void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            telechargementFini = true;
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Percent = e.ProgressPercentage;
        }

        private void updateIndex(int index)
        {
            if (index > 4)
            {
                index = 0;
            }
            else if (index < 0)
            {
                index = 4;
            }
            renderPictureBoxBG(imagesNews[index]);
            pagination[indexImage].Image = c._PAGINATION;
            pagination[index].Image = c._PAGINATION_OVER;
            indexImage = index;
        }

        private void pictureBoxFenetre_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void pictureBoxFermer_MouseHover(object sender, EventArgs e)
        {
            pictureBoxFermer.Image = c._FERMER_OVER;
        }

        private void pictureBoxFermer_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxFermer.Image = c._FERMER;
        }

        private void pictureBoxFermer_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }
            telechargementFini = true;
            extractionFini = true;
            Application.Exit();
        }

        private void pictureBoxReduire_MouseHover(object sender, EventArgs e)
        {
            pictureBoxReduire.Image = c._REDUIRE_OVER;
        }

        private void pictureBoxReduire_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxReduire.Image = c._REDUIRE;
        }

        private void pictureBoxReduire_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBoxReglages_MouseHover(object sender, EventArgs e)
        {
            pictureBoxReglages.Image = c._OPTION_OVER;
        }

        private void pictureBoxReglages_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxReglages.Image = c._OPTION;
        }

        private void pictureBoxPrev_MouseHover(object sender, EventArgs e)
        {
            pictureBoxPrev.Image = c._PREV_OVER;
        }

        private void pictureBoxPrev_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxPrev.Image = c._PREV;
        }

        private void pictureBoxNext_MouseHover(object sender, EventArgs e)
        {
            pictureBoxNext.Image = c._NEXT_OVER;
        }

        private void pictureBoxNext_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxNext.Image = c._NEXT;
        }

        private void pictureBoxSite_MouseHover(object sender, EventArgs e)
        {
            pictureBoxSite.Image = c._SITE_OVER;
        }

        private void pictureBoxSite_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxSite.Image = c._SITE;
        }

        private void pictureBoxForum_MouseHover(object sender, EventArgs e)
        {
            pictureBoxForum.Image = c._FORUM_OVER;
        }

        private void pictureBoxForum_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxForum.Image = c._FORUM;
        }

        private void pictureBoxVoter_MouseHover(object sender, EventArgs e)
        {
            pictureBoxVoter.Image = c._VOTER_HOVER;
        }

        private void pictureBoxVoter_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxVoter.Image = c._VOTER;
        }

        private void pictureBoxPag1_MouseHover(object sender, EventArgs e)
        {
            pictureBoxPag1.Image = c._PAGINATION_OVER;
        }

        private void pictureBoxPag1_MouseLeave(object sender, EventArgs e)
        {
            if (Index != 0)
            {
                pictureBoxPag1.Image = c._PAGINATION;
            }
        }

        private void pictureBoxPag1_Click(object sender, EventArgs e)
        {
            Index = 0;
        }

        private void pictureBoxPag2_MouseHover(object sender, EventArgs e)
        {
            pictureBoxPag2.Image = c._PAGINATION_OVER;
        }

        private void pictureBoxPag3_MouseHover(object sender, EventArgs e)
        {
            pictureBoxPag3.Image = c._PAGINATION_OVER;
        }

        private void pictureBoxPag4_MouseHover(object sender, EventArgs e)
        {
            pictureBoxPag4.Image = c._PAGINATION_OVER;
        }

        private void pictureBoxPag5_MouseHover(object sender, EventArgs e)
        {
            pictureBoxPag5.Image = c._PAGINATION_OVER;
        }

        private void pictureBoxPag2_MouseLeave(object sender, EventArgs e)
        {
            if (Index != 1)
            {
                pictureBoxPag2.Image = c._PAGINATION;
            }
        }

        private void pictureBoxPag3_MouseLeave(object sender, EventArgs e)
        {
            if (Index != 2)
            {
                pictureBoxPag3.Image = c._PAGINATION;
            }
        }

        private void pictureBoxPag4_MouseLeave(object sender, EventArgs e)
        {
            if (Index != 3)
            {
                pictureBoxPag4.Image = c._PAGINATION;
            }
        }

        private void pictureBoxPag5_MouseLeave(object sender, EventArgs e)
        {
            if (Index != 4)
            {
                pictureBoxPag5.Image = c._PAGINATION;
            }
        }

        private void pictureBoxPag2_Click(object sender, EventArgs e)
        {
            Index = 1;
        }

        private void pictureBoxPag3_Click(object sender, EventArgs e)
        {
            Index = 2;
        }

        private void pictureBoxPag4_Click(object sender, EventArgs e)
        {
            Index = 3;
        }

        private void pictureBoxPag5_Click(object sender, EventArgs e)
        {
            Index = 4;
        }

        private void pictureBoxPrev_Click(object sender, EventArgs e)
        {
            Index--;
        }

        private void pictureBoxNext_Click(object sender, EventArgs e)
        {
            Index++;
        }

        private void pictureBoxSite_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(siteWeb);
        }

        private void pictureBoxForum_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(forum);
        }

        private void pictureBoxVoter_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(voter);
        }

        private void pictureBoxJouer_MouseHover(object sender, EventArgs e)
        {
            pictureBoxJouer.Image = c._JOUER_OVER;
        }

        private void pictureBoxJouer_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxJouer.Image = c._JOUER;
        }

        private void pictureBoxReglages_Click(object sender, EventArgs e)
        {
            formSelection.ShowDialog();
        }

        private void pictureBoxJouer_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            Process.Start(Directory.GetCurrentDirectory().ToString() + "/Dofus.exe");
        }

        private void changerTheme()
        {
            themeNoir = !themeNoir;
            if (themeNoir)
            {
                c = new Constants();
            }
            else
            {
                c.changeThemePourBlanc();
            }
            this.BackgroundImage = c._MAIN_BG;
            renderPictureBoxBG(imagesNews[Index]);
        }

        private void FormUpdater_Shown(object sender, EventArgs e)
        {
            start();
        }

        /**
         * Code serialisation de données
         * On va peut-être ré-utiliser ces méthodes à d'autres fins
         **/
        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
                var fileContents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(fileContents);
            }
            catch
            {
                return default(T);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        private void FormUpdater_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon.BalloonTipText = "Le launcher est désormais dans la barre des tâches, juste ici";
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(500);
                this.Hide();
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                notifyIcon.Visible = false;
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void lancerDofusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Directory.GetCurrentDirectory().ToString() + "/Dofus.exe");
        }

        private void fermerLeLauncherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }
            telechargementFini = true;
            extractionFini = true;
            Application.Exit();
        }
    }

}
