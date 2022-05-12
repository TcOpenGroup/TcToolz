using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace BackupNow
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ShuttingDownWindow : Window
    {
        DispatcherTimer _timer;
        TimeSpan _time;
        public ShuttingDownWindow()
        {
            InitializeComponent();
            Topmost = true;
            Title = App.GetTitle();
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            PreviewKeyDown += new KeyEventHandler(HandleEsc);
            Closing += ShuttingDownWindow_Closing;

            _time = TimeSpan.FromSeconds(10);
            CountDownLabel.Content = _time.ToString("c");
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (_time == TimeSpan.Zero)
                {
                    _timer.Stop();

#if !DEBUG
            var psi = new ProcessStartInfo("shutdown", "/s /t 1");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
#endif
                    Process.GetCurrentProcess().Kill();

                }
                _time = _time.Add(TimeSpan.FromSeconds(-1));
                CountDownLabel.Content = _time.ToString("c");


            }, Application.Current.Dispatcher);
            _timer.Start();
        }

        private void ShuttingDownWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer.Stop();
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            _timer.Stop();
            Close();
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            Close();
        }
    }
}
