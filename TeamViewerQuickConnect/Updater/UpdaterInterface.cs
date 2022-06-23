using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Updater
{
    public class UpdaterInterface
    {
        static readonly Lazy<UpdaterInterface> _instance = new Lazy<UpdaterInterface>(() => new UpdaterInterface());

        public static UpdaterInterface Instance = _instance.Value;

        public void Run(UpdaterData data)
        {
            try
            {

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        HttpClient client = new HttpClient();
                        string ver = client.GetStringAsync(data.VersionFileUrl).Result;
                        string currentVer = data.CurrentVersion;
                        var version = new Version(ver);
                        var currentVersion = new Version(currentVer);
                        var result = version.CompareTo(currentVersion);
                        if (result > 0)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var w = new MainWindow();
                                w.Init(new MainViewModel(data));
                                w.ShowDialog();
                            });
                        }
                    }
                    catch (Exception)
                    {
                    }

                });
            }
            catch (Exception)
            {
            }
        }
    }

    public struct UpdaterData
    {
        public string VersionFileUrl { get; set; }
        public string SourceFileUrl { get; set; }
        public string TmpFilePath { get; set; }
        public string CurrentVersion { get; set; }
    }

}
