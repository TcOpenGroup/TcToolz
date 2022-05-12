using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Updater
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public UpdaterData _data { get; set; }

        public MainViewModel(UpdaterData data)
        {
            _data = data;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!Directory.Exists(_data.TmpFilePath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(_data.TmpFilePath));
                    }

                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        try
                        {
                            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                            wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                            wc.DownloadFileAsync(new Uri(_data.SourceFileUrl), _data.TmpFilePath);
                        }
                        catch (Exception ex)
                        {
                            ProgressMessage = ex.Message;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ProgressMessage = ex.Message;
                }
            });
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressMessage = "Downloading update";
            PercentageMessage = e.ProgressPercentage.ToString() + "%";
            ProgressPercentage = e.ProgressPercentage;

        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                ProgressMessage = "The update has been cancelled";
                return;
            }
            if (e.Error != null)
            {
                ProgressMessage = "An error ocurred while trying to update";
                return;
            }
            ProgressPercentage = 100;
            ProgressMessage = "Complete";
            Thread.Sleep(500);
            try
            {
                Process.Start(_data.TmpFilePath);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Process.GetCurrentProcess().Kill();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(System.Reflection.MethodBase.GetCurrentMethod().Name + Environment.NewLine +
                       ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private int _progressPercentage;
        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                _progressPercentage = value;
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }
        private string _percentageMessage;
        public string PercentageMessage
        {
            get { return _percentageMessage; }
            set
            {
                _percentageMessage = value;
                OnPropertyChanged(nameof(PercentageMessage));
            }
        }
        private string _progressMessage;
        public string ProgressMessage
        {

            get { return _progressMessage; }
            set
            {
                _progressMessage = value;
                OnPropertyChanged(nameof(ProgressMessage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
