using Autofac;
using BackupNow.DataAccess;
using Microsoft.Win32;
using ModernWpf;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupNow
{
    public class MainViewModel
    {
        public IDataService _dataService { get; set; }
        public AppSettingsWrapper Settings { get; set; }

        public MainViewModel()
        {
            Utils.RunWithErrorHandling(() =>
            {
                _dataService = Bootstrapper.Container.Resolve<IDataService>();
                Settings = new AppSettingsWrapper(_dataService.GetAppSettings());

                SetApplicationTheme(Settings.Theme);

                SetStartup();

                Settings.PropertyChanged += Settings_PropertyChanged;
            });
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SaveSettings();
        }

        public void SaveSettings()
        {
            Utils.RunWithErrorHandling(() =>
            {
                _dataService.SaveAppSettings(Settings.Model);
                SetApplicationTheme(Settings.Theme);

                SetStartup();
            });
        }

        private void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (Settings.RunOnStartup)
                rk.SetValue("BackupNow", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            else
                rk.DeleteValue("BackupNow", false);

        }

        public void SetApplicationTheme(Model.AppTheme theme)
        {
            if (theme == Model.AppTheme.Light)
            {
                SetApplicationTheme(ModernWpf.ApplicationTheme.Light);
            }
            else if (theme == Model.AppTheme.Dark)
            {
                SetApplicationTheme(ModernWpf.ApplicationTheme.Dark);
            }
            else if (theme == Model.AppTheme.System)
            {
                SetApplicationTheme(null);
            }
        }

        private void SetApplicationTheme(ApplicationTheme? theme)
        {
            DispatcherHelper.RunOnMainThread(() =>
            {
                ThemeManager.Current.ApplicationTheme = theme;
            });
        }
    }
}
