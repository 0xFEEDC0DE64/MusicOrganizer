using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MusicOrganizer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            objectListView1.AlwaysGroupByColumn = columnBpm;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string musicFolder;

                using (var dialog = new FolderBrowserDialog())
                {
                    var result = dialog.ShowDialog();

                    if (result != DialogResult.OK)
                        return;

                    if (string.IsNullOrWhiteSpace(dialog.SelectedPath))
                        return;

                    musicFolder = dialog.SelectedPath;
                }

                if (!musicFolder.EndsWith("\\") && !musicFolder.EndsWith("/"))
                    musicFolder += Path.DirectorySeparatorChar;

                var paths = Directory.GetFileSystemEntries(musicFolder, "*.mp3", SearchOption.AllDirectories);

                var objects = new List<object>();

                foreach (var path in paths)
                {
                    var file = TagLib.File.Create(path);

                    string relativePath;
                    if (path.StartsWith(musicFolder))
                        relativePath = path.Remove(0, musicFolder.Length);
                    else
                        relativePath = path;

                    objects.Add(new {
                        filename = relativePath,
                        artist = file.Tag.FirstPerformer,
                        title = file.Tag.Title,
                        bpm = string.Format("{0}BPM", file.Tag.BeatsPerMinute)
                    });
                }

                objectListView1.SetObjects(objects);
                objectListView1.AutoResizeColumns();

                button2.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}