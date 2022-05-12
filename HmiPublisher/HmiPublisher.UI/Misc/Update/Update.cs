namespace Libs.Update
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows;

    public class Update : Interaction
    {
        string sourceUrl;
        public string SourceUrl { get => sourceUrl; set => sourceUrl = value; }
        string destinationAppPath;
        public string DestinationAppPath { get => destinationAppPath; set => destinationAppPath = value; }
        public Update()
        {
            PercentageMessage = " ";
            ProgressMessage = " ";
            sourceUrl = "http://192.168.0.14:5551/_HmiPublisher_V2/HmiPublisherV2Setup.msi";
            destinationAppPath = System.IO.Path.GetTempPath() + @"HmiPublisherV2\HmiPublisherV2Setup.msi";
        }


        public async void UpdateTask()
        {
            await Task.Factory.StartNew(() =>
            {
                UpdateAsync();
            });
        }
        public bool UpdateAsync()
        {
            try
            {
                if (!Directory.Exists(destinationAppPath)) { Directory.CreateDirectory(Path.GetDirectoryName(destinationAppPath)); }

                DownloadFile(sourceUrl, destinationAppPath);
            }
            catch (Exception ex)
            {
                ProgressMessage = ex.Message;
                return false;
            }
            return true;
        }

        public void DownloadFile(string sourceUrl, string destFilePath)
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                    wc.DownloadFileAsync(new Uri(sourceUrl), destFilePath);
                }
                catch (Exception ex)
                {
                    ProgressMessage = ex.Message;
                }
            }
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
            ProgressMessage = "Everything is up to date";
            try
            {
                Process.Start(destinationAppPath);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Application.Current.Shutdown();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(System.Reflection.MethodBase.GetCurrentMethod().Name + Environment.NewLine +
                       ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

    }
}
