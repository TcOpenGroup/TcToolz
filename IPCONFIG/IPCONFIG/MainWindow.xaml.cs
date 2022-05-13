using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using System.Windows.Shapes;

namespace IPCONFIG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            _context = DataContext as MainViewModel;

            var ver = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString());

            Title = Application.ResourceAssembly.GetName().Name + " " + ver.Major + "." + ver.Minor;

            PreviewKeyDown += new KeyEventHandler(HandleEsc);

            PingTextBox.Focus();

        }

        MainViewModel _context { get; set; }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void NCPA_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("ncpa.cpl");
            }
            catch (Exception)
            {

            }
        }

        private void AutoIpComboBox_Click(object sender, RoutedEventArgs e)
        {
            SetButton.IsDefault = true;
            PingButton.IsDefault = false;
        }

        private void StaticIpComboBox_Click(object sender, RoutedEventArgs e)
        {
            SetButton.IsDefault = true;
            PingButton.IsDefault = false;
        }

        private void IPTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SetButton.IsDefault = true;
            PingButton.IsDefault = false;
        }

        private void SubnetMaskTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SetButton.IsDefault = true;
            PingButton.IsDefault = false;
        }

        private void PingIpTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SetButton.IsDefault = false;
            PingButton.IsDefault = true;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetButton.IsDefault = false;
            PingButton.IsDefault = true;
        }

        private void TextSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            SetButton.IsDefault = false;
            PingButton.IsDefault = false;

        }

        private void TextSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _context.SetCommand.Execute(_context.SelectedItem);
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _context.SetCommand.Execute(_context.SelectedItem);
        }
    }
}
