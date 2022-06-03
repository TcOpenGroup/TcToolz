using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BackupNow
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class InitWindow : Window
    {
        string _args = "";
        public InitWindow()
        {
            InitializeComponent();
            Topmost = true;
            Title = App.GetTitle() + " - Initialize";
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            PreviewKeyDown += new KeyEventHandler(HandleEsc);
            SubmitButton.Click += SubmitButton_Click;
            RemoveButton.Click += RemoveButton_Click;
            TextBox.Focus();
            AlreadyInitialized.Visibility = Visibility.Collapsed;
            RemoveButton.Visibility = Visibility.Collapsed;

            Utils.RunWithErrorHandling(() =>
            {
                _args = Utils.GetCommandLineArgs();
                if (!string.IsNullOrEmpty(_args))
                {
                    var existing = Directory.EnumerateFiles(_args, "*.backupnow", SearchOption.TopDirectoryOnly);
                    if (existing.Count() > 0)
                    {
                        AlreadyInitialized.Visibility = Visibility.Visible;
                        RemoveButton.Visibility = Visibility.Visible;
                        TextBox.Text = Path.GetFileNameWithoutExtension(existing.SingleOrDefault());
                        SubmitButton.Content = "Reinitialize";
                    }
                    else
                    {
                        var dirinfo = new DirectoryInfo(_args);
                        TextBox.Text = dirinfo.Name;
                    }

                    TextBox.SelectionStart = TextBox.Text.Length;
                    TextBox.SelectionLength = 0;
                }
            });
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.RunWithErrorHandling(() =>
            {
                foreach (string sFile in System.IO.Directory.GetFiles(_args, "*.backupnow", SearchOption.TopDirectoryOnly))
                {
                    System.IO.File.Delete(sFile);
                }
                Close();
            });
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.RunWithErrorHandling(() =>
            {
                if (!string.IsNullOrEmpty(TextBox.Text))
                {
                    foreach (string sFile in System.IO.Directory.GetFiles(_args, "*.backupnow", SearchOption.TopDirectoryOnly))
                    {
                        System.IO.File.Delete(sFile);
                    }

                    File.WriteAllLines(_args + @"\" + ReplaceInvalidChars(TextBox.Text) + ".backupnow", new string[] { });
                    Close();
                }
            });
        }

        public string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(System.IO.Path.GetInvalidFileNameChars()));
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
