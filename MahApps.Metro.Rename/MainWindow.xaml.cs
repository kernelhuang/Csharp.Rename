using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
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

        private string sourceDirPath    = "";
        private string destDirPath      = "";

        /// <summary>
        /// Initialized Component
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Show Message
        /// </summary>
        private void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
        }

        /// <summary>
        /// Reset fileList and sourceFiles and destFiles by List.
        /// </summary>
        private void Reset()
        {
            fileList       = new List<string>();
            sourceFiles    = new List<string>();
            destFiles      = new List<string>();
        }

        /// <summary>
        /// Checks whether the fileList is empty list.
        /// </summary>
        private bool FileList_Check()
        {
            if (fileList == null || fileList.Count == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether the sourceFiles is empty list.
        /// </summary>
        private bool SourceFiles_Check()
        {
            if (sourceFiles == null || sourceFiles.Count == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether the destFiles is empty list.
        /// </summary>
        private bool DestFiles_Check()
        {
            if (destFiles == null || destFiles.Count == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether sourceFiles or destFiles is empty list, And show message.
        /// </summary>
        private bool SourceFilesAndDestFiles_Check()
        {
            if (!SourceFiles_Check() || !DestFiles_Check())
            {
                string msg = "请重新选择源文件或文件夹";
                ShowMessage(msg);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether select files button, And show message.
        /// </summary>
        private bool SelectButton_Check()
        {
            if (!FileList_Check())
            {
                ShowMessage(SELECT_MESSAGE);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create target directory.
        /// </summary>
        private void CreateDestDir()
        {
            if (!Directory.Exists(destDirPath))
            {
                Directory.CreateDirectory(destDirPath);
            }
        }

        /// <summary>
        /// Copy from source file to target file.
        /// </summary>
        private void CopyFiles(string sourceFile, string destFile)
        {
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, destFile, true);
            }
        }

        /// <summary>
        /// Removed of the decimal point.
        /// </summary>
        private double ToChange(double value)
        {
            return value > 0 ? Math.Floor(value) : Math.Ceiling(value);
        }

        /// <summary>
        /// Checks whether firstInputBox or lastInputBox is empty.
        /// </summary>
        private bool InputBox_Check()
        {
            if (string.IsNullOrEmpty(firstInputBox.Text))
            {
                ShowMessage(FIRST_INPUT_MESSAGE);
                return false;
            }

            if (string.IsNullOrEmpty(lastInputBox.Text))
            {
                ShowMessage(LAST_INPUT_MESSAGE);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handle the submit button click event.
        /// </summary>
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InputBox_Check() || !SelectButton_Check() || !SourceFilesAndDestFiles_Check())
            {
                return;
            }

            CreateDestDir();
            int countFiles          = sourceFiles.Count;
            progressBar.Visibility  = Visibility.Visible;

            for(int i = 0; i < countFiles; i++)
            {
                double progressValue    = (Convert.ToDouble(i) + 1) / countFiles * 100;
                progressBar.Value       = ToChange(progressValue);
                CopyFiles(sourceFiles[i], destFiles[i]);

                if (i == countFiles -1 )
                {
                    ShowMessage("文件已经修改完毕");
                    progressBar.Value   = 0;
                    progressBar.Visibility = Visibility.Hidden;
                    Reset();
                    return;
                }
            }

            ShowMessage("文件修改失败，请重试");

            return;
        }

        /// <summary>
        /// Handle the select files button click event.
        /// </summary>
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InputBox_Check())
            {
                return;
            }

            Reset();
            fileListBox.Items.Clear();
            OpenFileDialog dialog   = new OpenFileDialog();
            dialog.Multiselect      = true;
            dialog.Title            = SELECT_FILE_TITLE;

            if (dialog.ShowDialog() == true)
            {
                sourceDirPath  = Path.GetDirectoryName(dialog.FileName);
                destDirPath    = sourceDirPath + "\\" + lastInputBox.Text;
                fileListBox.Items.Add(SOURCE_DIR_TITLE + sourceDirPath);
                string[] filename   = dialog.SafeFileNames;

                foreach(string name in filename)
                {
                    string extension = Path.GetExtension(name);
                    if (extension == "." + firstInputBox.Text)
                    {
                        sourceFiles.Add(sourceDirPath + "\\" + name);

                        string destFilename = Path.GetFileNameWithoutExtension(name);
                        if(destFilename.IndexOf(" ") >= 0) {
                            string newDestFilename = new Regex("[\\s]+").Replace(destFilename, "_");
                            destFiles.Add(sourceDirPath + "\\" + lastInputBox.Text + "\\" + newDestFilename + "." + lastInputBox.Text);
                        }
                        else
                        {
                            destFiles.Add(sourceDirPath + "\\" + lastInputBox.Text + "\\" + destFilename + "." + lastInputBox.Text);
                        }

                        fileList.Add(name);
                        fileListBox.Items.Add(DOT_POINT + name);
                    }
                }

                if (!FileList_Check())
                {
                    string msg = "请重新选择后缀名为：" + firstInputBox.Text + "的文件";
                    ShowMessage(msg);
                }
            }
            return;
        }

        /// <summary>
        /// Handle the select folder button click event.
        /// </summary>
        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InputBox_Check())
            {
                return;
            }

            Reset();
            fileListBox.Items.Clear();
            WinForms.FolderBrowserDialog folder = new WinForms.FolderBrowserDialog();

            if (folder.ShowDialog() == WinForms.DialogResult.OK)
            {
                sourceDirPath      = folder.SelectedPath;
                destDirPath        = sourceDirPath + "\\" + lastInputBox.Text;
                DirectoryInfo dirInfo   = new DirectoryInfo(sourceDirPath);
                FileInfo[] files        = dirInfo.GetFiles();
                fileListBox.Items.Add(SOURCE_DIR_TITLE + sourceDirPath);

                foreach (FileInfo file in files)
                {
                    string extension = Path.GetExtension(file.Name);
                    if (extension == "." + firstInputBox.Text)
                    {
                        sourceFiles.Add(sourceDirPath + "\\" + file.Name);
                        string destFilename = Path.GetFileNameWithoutExtension(file.Name);

                        if(destFilename.IndexOf(" ") >= 0) {
                            string newDestFilename = new Regex("[\\s]+").Replace(destFilename, "_");
                            destFiles.Add(sourceDirPath + "\\" + lastInputBox.Text + "\\" + newDestFilename + "." + lastInputBox.Text);
                        }
                        else
                        {
                            destFiles.Add(sourceDirPath + "\\" + lastInputBox.Text + "\\" + destFilename + "." + lastInputBox.Text);
                        }

                        fileList.Add(file.Name);
                        fileListBox.Items.Add(DOT_POINT + file.Name);
                    }
                }

                if (!FileList_Check())
                {
                    string msg = "此文件夹不存在后缀名为：" + firstInputBox.Text + "的文件，请重新选择文件夹";
                    ShowMessage(msg);
                }
            }
        }

        private void LastInputBox_TextInput(object sender, TextCompositionEventArgs e)
        {
        }
        
        private void FirstInputBox_TextInput(object sender, TextCompositionEventArgs e)
        {
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
    }
}
