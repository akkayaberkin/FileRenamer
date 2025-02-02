using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FileRenamer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form mainForm = new Form
            {
                Text = "File Renamer",
                Width = 500,
                Height = 300
            };

            Button btnSelectFolder = new Button
            {
                Text = "Klasör Seç",
                Left = 20,
                Top = 20,
                Width = 100
            };

            TextBox txtFolderPath = new TextBox
            {
                Left = 130,
                Top = 20,
                Width = 300
            };

            Button btnRename = new Button
            {
                Text = "Değiştir",
                Left = 20,
                Top = 120,
                Width = 100
            };

            TextBox txtOldName = new TextBox
            {
                Left = 130,
                Top = 60,
                Width = 300,
                PlaceholderText = "Eski İsmi Girin"
            };

            TextBox txtNewName = new TextBox
            {
                Left = 130,
                Top = 90,
                Width = 300,
                PlaceholderText = "Yeni İsmi Girin"
            };

            btnSelectFolder.Click += (sender, e) =>
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        txtFolderPath.Text = dialog.SelectedPath;
                    }
                }
            };

            btnRename.Click += (sender, e) =>
            {
                string folderPath = txtFolderPath.Text;
                string oldName = txtOldName.Text;
                string newName = txtNewName.Text;

                if (string.IsNullOrWhiteSpace(folderPath) ||
                    string.IsNullOrWhiteSpace(oldName) ||
                    string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("Lütfen tüm alanları doldurun!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    RenameFolders(folderPath, oldName, newName);
                    RenameFilesAndContent(folderPath, oldName, newName);
                    OpenFilesInNotepad(folderPath);

                    MessageBox.Show("Dosya, klasör adları ve içerikleri başarıyla değiştirildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            mainForm.Controls.Add(btnSelectFolder);
            mainForm.Controls.Add(txtFolderPath);
            mainForm.Controls.Add(txtOldName);
            mainForm.Controls.Add(txtNewName);
            mainForm.Controls.Add(btnRename);

            Application.Run(mainForm);
        }

        static void RenameFolders(string folderPath, string oldName, string newName)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

            foreach (var directory in dirInfo.GetDirectories("*", SearchOption.AllDirectories))
            {
                string newDirName = Regex.Replace(directory.Name, oldName, newName, RegexOptions.IgnoreCase);
                if (directory.Name != newDirName)
                {
                    string newDirPath = Path.Combine(directory.Parent.FullName, newDirName);
                    Directory.Move(directory.FullName, newDirPath);
                }
            }
        }

        static void RenameFilesAndContent(string folderPath, string oldName, string newName)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

            foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                string newFileName = Regex.Replace(file.Name, oldName, newName, RegexOptions.IgnoreCase);
                if (file.Name != newFileName)
                {
                    string newFilePath = Path.Combine(file.DirectoryName, newFileName);
                    File.Move(file.FullName, newFilePath);
                }
            }

            foreach (var file in dirInfo.GetFiles("*.*", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(file.FullName);
                string updatedContent = Regex.Replace(content, oldName, newName, RegexOptions.IgnoreCase);
                if (content != updatedContent)
                {
                    File.WriteAllText(file.FullName, updatedContent);
                }
            }

            foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                if (file.Extension.Equals("." + oldName, StringComparison.OrdinalIgnoreCase))
                {
                    string newFilePath = Path.Combine(file.DirectoryName, Path.GetFileNameWithoutExtension(file.Name) + "." + newName);
                    File.Move(file.FullName, newFilePath);
                }
            }
        }

        static void OpenFilesInNotepad(string folderPath)
        {
            foreach (var file in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{file}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
        }
    }
}
