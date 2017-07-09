using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MusicOrganizer
{
    internal partial class MainForm : Form
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
            using (var dialog = new CopyDialog())
                dialog.Execute(items);
        }
    }
}