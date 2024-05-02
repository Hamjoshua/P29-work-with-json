using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncAndLogFolders
{
    public partial class Form1 : Form
    {
        public SyncDirectory OriginalFolder;
        public SyncDirectory ImitatorFolder;

        ImageList _imageOriginalList;
        ImageList _imageImitatorList;

        public Form1()
        {
            InitializeComponent();
        }

        private string SelectFolderFromDialog()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Directory.GetCurrentDirectory();
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                return folderBrowserDialog.SelectedPath;
            }

            return null;
        }

        private void InitListViews(SyncDirectory directory, ref ListView listView, ref ImageList imageList)
        {
            listView.Items.Clear();
            imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            listView.SmallImageList = imageList;
            listView.View = View.SmallIcon;

            foreach (FileInfo fileInfo in directory.OldFiles)
            {
                AddItem(fileInfo, ref listView, ref imageList);
            }
        }

        private void AddItem(FileInfo fileInfo, ref ListView listView, ref ImageList imageList)
        {
            Icon icon = SystemIcons.WinLogo;
            ListViewItem item = new ListViewItem(fileInfo.Name, 1);

            if (!imageList.Images.ContainsKey(fileInfo.Name))
            {
                icon = Icon.ExtractAssociatedIcon(fileInfo.FullName);
                imageList.Images.Add(fileInfo.Name, icon);
            }

            item.ImageKey = fileInfo.Name;
            listView.Items.Add(item);
        }

        private void chooseOriginalFolderButton_Click(object sender, EventArgs e)
        {
            string path = SelectFolderFromDialog();
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Выбор папки прерван", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            OriginalFolder = new SyncDirectory(path);
            originalTextBox.Text = OriginalFolder.Directory.FullName;

            InitOriginalListView();
        }

        private void chooseImitatorFolderButton_Click(object sender, EventArgs e)
        {
            string path = SelectFolderFromDialog();
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Выбор папки прерван", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ImitatorFolder = new SyncDirectory(path);
            imitatorTextBox.Text = ImitatorFolder.Directory.FullName;

            InitImitatorListView();
        }

        void ColorApplied(ref ListView listView)
        {
            foreach (ListViewItem appliedItem in listView.Items)
            {
                appliedItem.ForeColor = Color.Black;
            }
        }

        void ColorChanges(SyncDirectory directory, ref ListView listView, ref ImageList imageList)
        {
            List<FileInfo> newFilesClone = directory.NewFiles.ToList();

            foreach (ListViewItem originalItem in listView.Items)
            {
                int indexOfExisingFile = newFilesClone.FindIndex(f => f.Name == originalItem.Text);

                if (directory.ChangedFiles.Contains(originalItem.Text) && indexOfExisingFile > -1)
                {
                    originalItem.ForeColor = Color.Orange;
                    originalItem.Text += " - Отредактирован";
                    newFilesClone.Remove(newFilesClone[indexOfExisingFile]);
                }
                else if (indexOfExisingFile == -1)
                {
                    originalItem.ForeColor = Color.Red;
                    originalItem.Text += "- Удален";
                }
                else
                {
                    newFilesClone.Remove(newFilesClone[indexOfExisingFile]);
                }
            }

            foreach (FileInfo fileInfo in newFilesClone)
            {
                AddItem(fileInfo, ref listView, ref imageList);
                ListViewItem addedItem = listView.Items[listView.Items.Count - 1];
                addedItem.ForeColor = Color.Green;
                addedItem.Text += " - Создан";
            }
        }

        private void syncTwoFouldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OriginalFolder == null || ImitatorFolder == null)
            {
                MessageBox.Show("Не выбраны папки");
                return;
            }

            Refresh();

            OriginalFolder.SyncWith(ImitatorFolder, false, false);
            ColorChanges(OriginalFolder, ref originalListView, ref _imageOriginalList);

            ImitatorFolder.SyncWith(OriginalFolder, false, false);
            ColorChanges(ImitatorFolder, ref imitatorListView, ref _imageImitatorList);

            OriginalFolder.ApplyChanges();
            ImitatorFolder.ApplyChanges();

            MessageBox.Show("Синхронизация завершена!");
        }

        void RefreshDirectories()
        {
            OriginalFolder.ApplyChanges();
            ImitatorFolder.ApplyChanges();
            InitOriginalListView();
            InitImitatorListView();
        }

        private void оригиналПоИмитаторуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OriginalFolder == null || ImitatorFolder == null)
            {
                MessageBox.Show("Не выбраны папки");
                return;
            }

            RefreshDirectories();

            OriginalFolder.SyncWith(ImitatorFolder, true, true);
            ColorChanges(OriginalFolder, ref originalListView, ref _imageOriginalList);

            OriginalFolder.ApplyChanges();
            InitImitatorListView();

            MessageBox.Show("Синхронизация завершена!");
        }

        private void syncImitatorWithOriginalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OriginalFolder == null || ImitatorFolder == null)
            {
                MessageBox.Show("Не выбраны папки");
                return;
            }

            Refresh();

            ImitatorFolder.SyncWith(OriginalFolder, true, true);
            ColorChanges(ImitatorFolder, ref imitatorListView, ref _imageImitatorList);

            ImitatorFolder.ApplyChanges();
            InitOriginalListView();

            MessageBox.Show("Синхронизация завершена!");
        }

        void InitImitatorListView()
        {
            InitListViews(ImitatorFolder, ref imitatorListView, ref _imageImitatorList);
        }
        void InitOriginalListView()
        {
            InitListViews(OriginalFolder, ref originalListView, ref _imageOriginalList);
        }
    }
}
