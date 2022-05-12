

namespace Libs.Update
{
    using HmiPublisher;
    using Libs.Other;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Windows;

    public class CheckForUpdate
    {
        public async void CheckForUpdateTask()
        {
            await Task.Factory.StartNew(() =>
            {
                CheckForUpdateAsync();
            });
        }
        public bool CheckForUpdateAsync()
        {
            try
            {
                HttpClient client = new HttpClient();
                string ver = (client.GetStringAsync("http://192.168.0.14:5551/_HmiPublisher_V2/version.txt").Result);
                string currentVer = (CurrentApp.GetVersion());
                var version = new Version(ver);
                var currentVersion = new Version(currentVer);
                var result = version.CompareTo(currentVersion);
                if (result > 0)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        UpdateWindow update = new UpdateWindow();
                        update.ShowDialog();
                    });
                }
            }
            catch (Exception ex)
            {
            }
            return true;
        }
    }
}
