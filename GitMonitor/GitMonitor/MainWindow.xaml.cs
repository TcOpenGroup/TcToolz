using GitMonitor.Model;
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

namespace GitMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _context { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var ver = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Title = "GitMonitor " + ver.Major + "." + ver.Minor;

            DataContext = new MainViewModel();
            _context = DataContext as MainViewModel;

            PreviewKeyDown += new KeyEventHandler(HandleEsc);
            Closing += MainWindow_Closing;

            DataGrid.MouseDoubleClick += DataGrid_MouseDoubleClick;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _context.ShowDetailsCommand.Execute(null);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
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
