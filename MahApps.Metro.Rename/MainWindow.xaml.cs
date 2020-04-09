using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;
using System.Text.RegularExpressions;

namespace MahApps.Metro.Rename
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly string DOT_POINT           = "⚫ ";
        private readonly string SELECT_FILE_TITLE   = "选择文件";
        private readonly string SOURCE_DIR_TITLE    = "源文件目录：";
        private readonly string FIRST_INPUT_MESSAGE = "修改前的文件类型必填";
        private readonly string LAST_INPUT_MESSAGE  = "修改后的文件类型必填";
        private readonly string SELECT_MESSAGE      = "请选择源文件或文件夹";

        private List<string> fileList       = new List<string>();
        private List<string> sourceFiles    = new List<string>();
        private List<string> destFiles      = new List<string>();

        private string firstInputText   = "";
        private string lastInputText    = "";
        private string sourceDirPath    = "";
        private string destDirPath      = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
        }

        private void Reset()
        {
            this.fileList       = new List<string>();
            this.sourceFiles    = new List<string>();
            this.destFiles      = new List<string>();
        }

        private bool FileList_Check()
        {
            if (this.fileList == null || this.fileList.Count == 0)
            {
                return false;
            }

            return true;
        }

        private bool SourceFiles_Check()
        {
            if (this.sourceFiles == null || this.sourceFiles.Count == 0)
            {
                return false;
            }

            return true;
        }

        private bool DestFiles_Check()
        {
            if (this.destFiles == null || this.destFiles.Count == 0)
            {
                return false;
            }

            return true;
        }

        private bool SourceFilesAndDestFiles_Check()
        {
            if (!this.SourceFiles_Check() || !this.DestFiles_Check())
            {
                string msg = "请重新选择源文件或文件夹";
                this.ShowMessage(msg);
                return false;
            }

            return true;
        }

        private bool SelectButton_Check()
        {
            if (!this.FileList_Check())
            {
                this.ShowMessage(SELECT_MESSAGE);
                return false;
            }

            return true;
        }

        private void CreateDestDir()
        {
            if (!Directory.Exists(this.destDirPath))
            {
                Directory.CreateDirectory(this.destDirPath);
            }
        }

        private void CopyFiles(string sourceFile, string destFile)
        {
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, destFile, true);
            }
        }

        private double ToChange(double value)
        {
            return value > 0 ? Math.Floor(value) : Math.Ceiling(value);
        }

        private bool InputBox_Check()
        {
            if (string.IsNullOrEmpty(firstInputBox.Text))
            {
                this.ShowMessage(FIRST_INPUT_MESSAGE);
                return false;
            }

            if (string.IsNullOrEmpty(lastInputBox.Text))
            {
                this.ShowMessage(LAST_INPUT_MESSAGE);
                return false;
            }

            return true;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.InputBox_Check())
            {
                return;
            }

            if (!this.SelectButton_Check())
            {
                return;
            }

            if (!this.SourceFilesAndDestFiles_Check())
            {
                return;
            }

            this.CreateDestDir();
            int countFiles          = this.sourceFiles.Count;
            progressBar.Visibility  = System.Windows.Visibility.Visible;

            for(int i = 0; i < countFiles; i++)
            {
                double progressValue    = ((Convert.ToDouble(i) + 1) / countFiles) * 100;
                progressBar.Value       = this.ToChange(progressValue);
                this.CopyFiles(this.sourceFiles[i], this.destFiles[i]);

                if (i == (countFiles -1 ))
                {
                    this.ShowMessage("文件已经修改完毕");
                    progressBar.Value   = 0;
                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    this.Reset();
                    return;
                }
            }

            this.ShowMessage("文件修改失败，请重试");

            return;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.InputBox_Check())
            {
                return;
            }

            this.Reset();
            fileListBox.Items.Clear();
            OpenFileDialog dialog   = new OpenFileDialog();
            dialog.Multiselect      = true;
            dialog.Title            = SELECT_FILE_TITLE;

            if (dialog.ShowDialog() == true)
            {
                this.sourceDirPath  = Path.GetDirectoryName(dialog.FileName);
                this.destDirPath    = this.sourceDirPath + "\\" + lastInputBox.Text;
                fileListBox.Items.Add(SOURCE_DIR_TITLE + this.sourceDirPath);
                string[] filename   = dialog.SafeFileNames;

                foreach(string name in filename)
                {
                    string extension = Path.GetExtension(name);
                    if (extension == ("." + firstInputBox.Text))
                    {
                        this.sourceFiles.Add(this.sourceDirPath + "\\" + name);

                        string destFilename = Path.GetFileNameWithoutExtension(name);
                        if(destFilename.IndexOf(" ") >= 0) {
                            string newDestFilename = new Regex("[\\s]+").Replace(destFilename, "_");
                            this.destFiles.Add(this.sourceDirPath + "\\" + lastInputBox.Text + "\\" + newDestFilename + "." + lastInputBox.Text);
                        }
                        else
                        {
                            this.destFiles.Add(this.sourceDirPath + "\\" + lastInputBox.Text + "\\" + destFilename + "." + lastInputBox.Text);
                        }

                        this.fileList.Add(name);
                        fileListBox.Items.Add(DOT_POINT + name);
                    }
                }

                if (!this.FileList_Check())
                {
                    string msg = "请重新选择后缀名为：" + firstInputBox.Text + "的文件";
                    this.ShowMessage(msg);
                }
            }
            return;
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.InputBox_Check())
            {
                return;
            }

            this.Reset();
            fileListBox.Items.Clear();
            WinForms.FolderBrowserDialog folder = new WinForms.FolderBrowserDialog();

            if (folder.ShowDialog() == WinForms.DialogResult.OK)
            {
                this.sourceDirPath      = folder.SelectedPath;
                this.destDirPath        = this.sourceDirPath + "\\" + lastInputBox.Text;
                DirectoryInfo dirInfo   = new DirectoryInfo(this.sourceDirPath);
                FileInfo[] files        = dirInfo.GetFiles();
                fileListBox.Items.Add(SOURCE_DIR_TITLE + this.sourceDirPath);

                foreach (FileInfo file in files)
                {
                    string extension = Path.GetExtension(file.Name);
                    if (extension == ("." + firstInputBox.Text))
                    {
                        this.sourceFiles.Add(this.sourceDirPath + "\\" + file.Name);
                        string destFilename = Path.GetFileNameWithoutExtension(file.Name);

                        if(destFilename.IndexOf(" ") >= 0) {
                            string newDestFilename = new Regex("[\\s]+").Replace(destFilename, "_");
                            this.destFiles.Add(this.sourceDirPath + "\\" + lastInputBox.Text + "\\" + newDestFilename + "." + lastInputBox.Text);
                        }
                        else
                        {
                            this.destFiles.Add(this.sourceDirPath + "\\" + lastInputBox.Text + "\\" + destFilename + "." + lastInputBox.Text);
                        }

                        this.fileList.Add(file.Name);
                        fileListBox.Items.Add(DOT_POINT + file.Name);
                    }
                }

                if (!this.FileList_Check())
                {
                    string msg = "此文件夹不存在后缀名为：" + firstInputBox.Text + "的文件，请重新选择文件夹";
                    this.ShowMessage(msg);
                }
            }
        }

        private void LastInputBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            this.lastInputText = lastInputBox.Text;
        }
        
        private void FirstInputBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            this.firstInputText = firstInputBox.Text;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
    }
}
