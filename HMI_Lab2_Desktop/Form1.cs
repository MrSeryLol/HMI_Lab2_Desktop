using System.IO;
using System;
using System.Windows.Forms;

namespace HMI_Lab2_Desktop
{
    public partial class Form1 : Form
    {
        private DirectoryInfo _directoryInfo;
        private string _oldFileName;
        private string _oldFullFileName;
        private string _newFullFileName;    
        public Form1()
        {
            InitializeComponent();
            PopulateTreeView();
        }

        private void PopulateTreeView()
        {
            TreeNode rootNode;

            DirectoryInfo info = new DirectoryInfo(@"C:/HMI_Lab2");
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                GetDirectories(info.GetDirectories(), rootNode);
                treeView1.Nodes.Add(rootNode);
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs,
            TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";

                //try
                //{
                //    subSubDirs = subDir.GetDirectories();
                //    if (subSubDirs.Length != 0)
                //    {
                //        GetDirectories(subSubDirs, aNode);
                //    }
                //    nodeToAddTo.Nodes.Add(aNode);
                //}
                //catch(Exception ex)
                //{
                //    Console.WriteLine(ex.ToString());
                //    break;
                //}
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                    {new ListViewItem.ListViewSubItem(item, "Directory"),
             new ListViewItem.ListViewSubItem(item,
                dir.LastAccessTime.ToShortDateString())};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                    { new ListViewItem.ListViewSubItem(item, "File"),
             new ListViewItem.ListViewSubItem(item,
                file.LastAccessTime.ToShortDateString())};

                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            _directoryInfo = nodeDirInfo;
        }

        static bool isExcluded(List<string> exludedDirList, string target)
        {
            return exludedDirList.Any(d => new DirectoryInfo(target).Name.Equals(d));
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var focusedItem = listView1.FocusedItem;
                if (focusedItem != null && focusedItem.Bounds.Contains(e.Location) && focusedItem.SubItems[1].Text == "File")
                {
                    listView1.LabelEdit = true;
                }
            }
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            listView1.LabelEdit = false;
            string newFileName;

            if (e.Label == null)
            {
                newFileName = Path.GetFileNameWithoutExtension(_directoryInfo.FullName + "\\" + listView1.FocusedItem.SubItems[0].Text);
                _newFullFileName = _directoryInfo.FullName + "\\" + listView1.FocusedItem.SubItems[0].Text;
            }
            else
            {
                newFileName = Path.GetFileNameWithoutExtension(_directoryInfo.FullName + "\\" + e.Label);
                _newFullFileName = _directoryInfo.FullName + "\\" + e.Label;
            }

            if (_oldFileName != newFileName)
            {
                MessageBox.Show("Вы пытаетесь изменить название файла!");
                e.CancelEdit = true;
                return;
            }

            if (_newFullFileName == _oldFullFileName)
            {
                return;
            }

            File.Move(_oldFullFileName, _newFullFileName);
        }

        private void listView1_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            var focusedItem = listView1.FocusedItem;
            _oldFileName = Path.GetFileNameWithoutExtension(_directoryInfo.FullName + "\\" + focusedItem.SubItems[0].Text);
            _oldFullFileName = _directoryInfo.FullName + "\\" + focusedItem.SubItems[0].Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Напишите, какие файлы вы хотите изменить!");
                return;
            }

            for (var i = 0; i < listView1.Items.Count; i++)
            {
                var item = listView1.Items[i];

                if (item.SubItems[1].Text == "File" && item.Text.StartsWith(textBox1.Text))
                {
                    listView1.SelectedIndices.Add(i);
                }
            }

            button2.Enabled = true;
            textBox2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string newFileName;
            List<string> changedFiles = new List<string>();
            List<string> oldFiles = new List<string>();

            if (textBox2.Text == "")
            {
                MessageBox.Show("Вы не написали новое расширение");
                button2.Enabled = false;
                textBox2.Enabled = false;
                return;
            }

            foreach (var item in listView1.SelectedIndices)
            {
                _oldFileName = Path.GetFullPath(_directoryInfo.FullName + "\\" + listView1.Items[(int)item].SubItems[0].Text);
                oldFiles.Add(_oldFileName);
                newFileName = Path.GetFileNameWithoutExtension(_directoryInfo.FullName + "\\" + listView1.Items[(int)item].SubItems[0].Text);
                _newFullFileName = _directoryInfo.FullName + "\\" + newFileName + "." + textBox2.Text;
                changedFiles.Add(_newFullFileName);

            }

            for (int i = 0; i < changedFiles.Count; i++)
            {
                File.Move(oldFiles[i], changedFiles[i]);
            }
        }
    }
}