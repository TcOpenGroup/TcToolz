using Autofac;
using ModernWpf;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Updater;

namespace BackupNow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string GetTitle()
        {
            return "Backup Now";
        }

        public static string GetVersion()
        {
            var ver = new Version(Utils.GetVersion());
            return ver.Major + "." + ver.Minor;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            UpdaterInterface.Instance.Run(new UpdaterData
            {
                VersionFileUrl = "http://192.168.0.14:5551/BackupNow/version.txt",
                ReleaseNotesUrl = "http://192.168.0.14:5551/BackupNow/release_notes.txt",
                SourceFileUrl = "http://192.168.0.14:5551/BackupNow/BackupNow_SETUP.exe",
                TmpFilePath = Path.GetTempPath() + @"BackupNow\BackupNow_SETUP.exe",
                CurrentVersion = Utils.GetVersion(),
            });

            new Bootstrapper();

            if (!string.IsNullOrEmpty(Utils.GetCommandLineArgs()))
            {
                var d = new InitWindow();
                d.ShowDialog();
                Process.GetCurrentProcess().Kill();
            }


            //#if !DEBUG
            //            var currdir = AppDomain.CurrentDomain.BaseDirectory;
            //            var paths = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
            //            var ps = paths.Split(';');
            //            if (!ps.Any(x => x == currdir))
            //            {
            //                Environment.SetEnvironmentVariable("Path", paths + ";" + currdir, EnvironmentVariableTarget.User);
            //            }
            //#endif
        }
    }
}
