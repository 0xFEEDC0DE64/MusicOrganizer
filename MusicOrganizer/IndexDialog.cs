using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MusicOrganizer
{
    internal partial class IndexDialog : Form
    {
        private string musicFolder;
        private List<Item> items;
        private string[] paths;

        public List<Item> Items { get { return items; } }

        public IndexDialog()
        {
            InitializeComponent();
        }

        public DialogResult Execute()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                    return DialogResult.Cancel;

                if (string.IsNullOrWhiteSpace(dialog.SelectedPath))
                    return DialogResult.Cancel;

                musicFolder = dialog.SelectedPath;
            }

            if (!musicFolder.EndsWith("\\") && !musicFolder.EndsWith("/"))
                musicFolder += Path.DirectorySeparatorChar;

            again0:
            try
            {
                paths = Directory.GetFileSystemEntries(musicFolder, "*.mp3", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                var result = MessageBox.Show(string.Format("Error occured when collecting files\n\n{0}", ex.Message), "Error occured", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                switch (result)
                {
                    case DialogResult.Retry: goto again0;
                    case DialogResult.Cancel: return DialogResult.Cancel;
                    default: throw new Exception("Unknown option clicked.");
                }
            }

            return ShowDialog();
        }

        private void IndexDialog_Load(object sender, EventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }

        private void IndexDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorker.IsBusy)
            {
                e.Cancel = true;
                if (!backgroundWorker.CancellationPending)
                    backgroundWorker.CancelAsync();
            }
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy)
            {
                if(!backgroundWorker.CancellationPending)
                    backgroundWorker.CancelAsync();
                cancelButton.Enabled = false;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            items = new List<Item>();

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                var statusStr = Path.GetFileName(path) + ": ";

                if(worker.CancellationPending)
                {
                    worker.ReportProgress(i * 100 / paths.Length, statusStr + "Cancelled!");
                    e.Cancel = true;
                    return;
                }

                TagLib.File file;
                
                try
                {
                    file = TagLib.File.Create(path);
                }
                catch (Exception ex)
                {
                    worker.ReportProgress(i * 100 / paths.Length, statusStr + "Exception!\r\n" + ex.Message);
                    Thread.Sleep(5000);
                    continue;
                }

                string relativePath;
                if (path.StartsWith(musicFolder))
                    relativePath = path.Remove(0, musicFolder.Length);
                else
                    relativePath = path;

                items.Add(new Item
                {
                    filename = path,
                    relativeFilename = relativePath,
                    artist = file.Tag.FirstPerformer,
                    title = file.Tag.Title,
                    bpm = string.Format("{0}BPM", file.Tag.BeatsPerMinute)
                });

                worker.ReportProgress((i + 1) * 100 / paths.Length, statusStr + "Ok!");
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            statusLabel.Text = e.UserState as string;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else
            {
                cancelButton.Enabled = false;
                continueButton.Enabled = true;
            }
        }
    }
}
