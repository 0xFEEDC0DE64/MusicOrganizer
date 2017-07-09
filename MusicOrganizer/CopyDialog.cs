using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace MusicOrganizer
{
    internal partial class CopyDialog : Form
    {
        private List<Item> items;
        private string targetFolder;

        public CopyDialog()
        {
            InitializeComponent();
        }

        public DialogResult Execute(List<Item> items)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                    return DialogResult.Cancel;

                if (string.IsNullOrWhiteSpace(dialog.SelectedPath))
                    return DialogResult.Cancel;

                targetFolder = dialog.SelectedPath;
            }

            this.items = items;

            return ShowDialog();
        }

        private void CopyDialog_Load(object sender, EventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }

        private void CopyDialog_FormClosing(object sender, FormClosingEventArgs e)
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
                if (!backgroundWorker.CancellationPending)
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

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var statusStr = Path.GetFileName(item.filename) + ": ";

                if (worker.CancellationPending)
                {
                    worker.ReportProgress((i + 1) * 100 / items.Count, statusStr + "Cancelled!");
                    e.Cancel = true;
                    return;
                }

                var bpmPath = Path.Combine(targetFolder, item.bpm);

                if (!Directory.Exists(bpmPath))
                    try
                    {
                        worker.ReportProgress((i + 1) * 100 / items.Count, statusStr + "Creating bpm folder...");
                        Directory.CreateDirectory(bpmPath);
                    }
                    catch (Exception ex)
                    {
                        worker.ReportProgress((i + 1) * 100 / items.Count, statusStr + "Exception:\r\n" + ex.Message);
                        continue;
                    }

                var target = Path.Combine(bpmPath, Path.GetFileName(item.filename));

                worker.ReportProgress((i + 1) * 100 / items.Count, statusStr + "Copying...");

                try
                {
                    File.Copy(item.filename, target);
                }
                catch (Exception ex)
                {
                    worker.ReportProgress((i + 1) * 100 / items.Count, statusStr + "Exception:\r\n" + ex.Message);
                    continue;
                }
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            statusLabel.Text = e.UserState as string;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
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