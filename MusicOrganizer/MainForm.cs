using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MusicOrganizer
{
    public partial class MainForm : Form
    {
        private List<Item> items;

        public MainForm()
        {
            InitializeComponent();

            objectListView1.AlwaysGroupByColumn = columnBpm;
            items = new List<Item>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var dialog = new IndexDialog())
            {
                if(dialog.Execute() == DialogResult.OK)
                {
                    items = dialog.Items;
                    objectListView1.SetObjects(items);
                    objectListView1.AutoResizeColumns();

                    button2.Enabled = items.Count > 0;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string targetFolder;

            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                    return;

                if (string.IsNullOrWhiteSpace(dialog.SelectedPath))
                    return;

                targetFolder = dialog.SelectedPath;
            }

            foreach(var item in items)
            {
                var bpmPath = Path.Combine(targetFolder, item.bpm);

                again0:
                if(!Directory.Exists(bpmPath))
                    try
                    {
                        Directory.CreateDirectory(bpmPath);
                    }
                    catch(Exception ex)
                    {
                        var result = MessageBox.Show(string.Format("Error occured when creating bpm folder for file\n\n{0}\n\n{1}", item.filename, ex.Message), "Error occured", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);

                        switch (result)
                        {
                            case DialogResult.Abort: return;
                            case DialogResult.Retry: goto again0;
                            case DialogResult.Ignore: continue;
                            default: throw new Exception("Unknown option clicked.");
                        }
                    }

                var target = Path.Combine(bpmPath, Path.GetFileName(item.filename));

                again1:
                try
                {
                    File.Copy(item.filename, target);
                }
                catch(Exception ex)
                {
                    var result = MessageBox.Show(string.Format("Error occured when copying file\n\n{0}\n\nto bpmfolder\n\n{1}\n\n{2}", item.filename, target, ex.Message), "Error occured", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);

                    switch (result)
                    {
                        case DialogResult.Abort: return;
                        case DialogResult.Retry: goto again1;
                        case DialogResult.Ignore: continue;
                        default: throw new Exception("Unknown option clicked.");
                    }
                }
            }
        }
    }
}