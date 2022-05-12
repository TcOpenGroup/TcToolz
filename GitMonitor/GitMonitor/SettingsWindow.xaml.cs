using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GitMonitor
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool Save;

        public SettingsWindow(AppSettingsWrapper model)
        {
            InitializeComponent();
            DataContext = model;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainViewModel.Instance.Settings = Utils.Clone(DataContext as AppSettingsWrapper);
            MainViewModel.Instance.SaveSettings();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer", System.IO.Path.GetDirectoryName(System.IO.Path.GetTempPath() + @"\GitMonitor\Settings.json"));
            }
            catch (Exception)
            {

            }
        }
    }
}
