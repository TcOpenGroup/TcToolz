using HmiPublisher.DataAccess;
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

namespace HmiPublisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var ver = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString());

            if (string.IsNullOrEmpty(Utils.GetCommandLineArgs()))
            {
                Title = App.GetTitle();
            }
            else
            {
                Title = Title = App.GetTitle() + " - " + Utils.GetCommandLineArgs();
            }


            DataContext = new MainViewModel();
            _context = DataContext as MainViewModel;

            PreviewKeyDown += new KeyEventHandler(HandleEsc);
            Closing += MainWindow_Closing;

            ListBox1.MouseDoubleClick += ListBox_MouseDoubleClick;

            ListBox1.Focus();


        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _context.SwitchViewCommand.Execute(null);
        }

        bool initialized = false;
        private void CboxEnum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initialized)
            {
                initialized = true;
                return;
            }
            //_context.SaveSettings();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_context.InProgress)
            {
                var res = MessageBox.Show("Do you want to close the app?", Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                _context.CancelCommand.Execute(null);
            }
            Process.GetCurrentProcess().Kill();
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_context.InProgress)
                {
                    _context.CancelCommand.Execute(null);
                }
                else
                {
                    Close();
                }
            }
        }

        MainViewModel _context { get; set; }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
    }
}
