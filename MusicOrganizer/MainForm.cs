using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MusicOrganizer
{
    public partial class MainForm : Form
    {
        struct Item
        {
            public string filename;
            public string relativeFilename;
            public string artist;
            public string title;
            public string bpm;
        }

        private List<Item> items;

        public MainForm()
        {
            InitializeComponent();

            objectListView1.AlwaysGroupByColumn = columnBpm;
            items = new List<Item>();
        }

        private void button1_Click(object sender, EventArgs e)
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

            string[] paths;

            again1:
            try
            {
                paths = Directory.GetFileSystemEntries(musicFolder, "*.mp3", SearchOption.AllDirectories);
            }
            catch(Exception ex)
            {
                var result = MessageBox.Show(string.Format("Error occured when collecting files\n\n{0}", ex.Message), "Error occured", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                switch(result)
                {
                    case DialogResult.Retry: goto again1;
                    case DialogResult.Cancel: return;
                    default: throw new Exception("Unknown option clicked.");
                }
            }

            var newItems = new List<Item>();

            foreach (var path in paths)
            {
                TagLib.File file;

                again2:
                try
                {
                    file = TagLib.File.Create(path);
                }
                catch(Exception ex)
                {
                    var result = MessageBox.Show(string.Format("Error occured when processing file\n\n{0}\n\n{1}", path, ex.Message), "Error occured", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);

                    switch(result)
                    {
                        case DialogResult.Abort: return;
                        case DialogResult.Retry: goto again2;
                        case DialogResult.Ignore: continue;
                        default: throw new Exception("Unknown option clicked.");
                    }
                }

                string relativePath;
                if (path.StartsWith(musicFolder))
                    relativePath = path.Remove(0, musicFolder.Length);
                else
                    relativePath = path;

                newItems.Add(new Item
                {
                    filename = path,
                    relativeFilename = relativePath,
                    artist = file.Tag.FirstPerformer,
                    title = file.Tag.Title,
                    bpm = string.Format("{0}BPM", file.Tag.BeatsPerMinute)
                });
            }

            items = newItems;
            objectListView1.SetObjects(items);
            objectListView1.AutoResizeColumns();

            button2.Enabled = items.Count > 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}