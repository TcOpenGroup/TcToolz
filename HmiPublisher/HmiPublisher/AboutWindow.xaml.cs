using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HmiPublisher
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            Topmost = true;
            Title = "About";
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            PreviewKeyDown += new KeyEventHandler(HandleEsc);
            SubmitButton.Click += SubmitButton_Click;
            VersionLabel.Content = "Version: " + App.GetVersion();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
