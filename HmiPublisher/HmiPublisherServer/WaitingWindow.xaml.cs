using System;
using System.Reflection;
using System.Windows;

namespace HmiPublisherServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WaitingWindow : Window
    {
        public WaitingWindow()
        {
            InitializeComponent();

            var ver = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString());

            labVersion.Text = $"Version: " + ver.Major + "." + ver.Minor;
            this.Topmost = true;

            labCopyright.Text = "© " + DateTime.Now.Year + " MTS spol. s.r.o.";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
    }
}
