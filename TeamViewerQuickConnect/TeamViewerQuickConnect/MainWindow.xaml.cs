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

namespace QuickConnect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            _context = DataContext as MainViewModel;


            var ver = new Version(Utils.GetVersion());

            Title = Application.ResourceAssembly.GetName().Name + " " + ver.Major + "." + ver.Minor;

            PreviewKeyDown += new KeyEventHandler(HandleEsc);

            Closing += MainWindow_Closing;

            TextSearch1.PreviewKeyDown += TextSearch1_PreviewKeyDown;
            TextSearch1.Focus();

            if (_context.Settings.Model.SizeW > 0)
            {
                //if (_context.Settings.Model.RememberLocation)
                //{
                //    Left = _context.Settings.Model.LocationX;
                //    Top = _context.Settings.Model.LocationY;
                //}

                if (_context.Settings.Model.RememberSize)
                {
                    Width = _context.Settings.Model.SizeW;
                    Height = _context.Settings.Model.SizeH;
                }
            }

            //if (!_context.Settings.Model.RememberLocation)
            //{
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //}

            Instance = this;

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _context.Settings.Model.SizeW = Width;
            _context.Settings.Model.SizeH = Height;

            _context.Settings.Model.LocationX = Left;
            _context.Settings.Model.LocationY = Top;

            _context.SaveSettings();
        }

        private void TextSearch1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _context.RunFitered();
            }
            else if (e.Key == Key.Down)
            {

                if (_context.Items.Count() > 0)
                {
                    ListBox.Focus();
                }
            }
        }


        MainViewModel _context { get; set; }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }


        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _context.Run();
            }
            catch (Exception)
            {
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            _context.SaveSettings();
        }

        private void TextSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void ListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var listbox = sender as ListBox;
            if (e.Key == Key.Enter)
            {
                _context.Run();
            }
            else if (e.Key == Key.Up)
            {
                var filteredItems = _context.Items.Cast<ItemWrapper>();
                if (ReferenceEquals(_context.SelectedItem, filteredItems.FirstOrDefault()))
                {
                    TextSearch1.Focus();
                }
            }
        }

        private void ClearTextBoxButton_Click(object sender, RoutedEventArgs e)
        {
            TextSearch1.Clear();
            TextSearch1.Focus();
        }
    }
}
