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
            var ver = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString());
            return ver.Major + "." + ver.Minor;
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Process curr = Process.GetCurrentProcess();
            //Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            //foreach (Process p in procs)
            //{
            //    if ((p.Id != curr.Id) && (p.MainModule.FileName == curr.MainModule.FileName))
            //    {
            //        Environment.Exit(0);
            //    }
            //}

#if !DEBUG
            var currdir = AppDomain.CurrentDomain.BaseDirectory;
            var paths = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
            var ps = paths.Split(';');
            if (!ps.Any(x => x == currdir))
            {
                Environment.SetEnvironmentVariable("Path", paths + ";" + currdir, EnvironmentVariableTarget.User);
            }
#endif
        }
    }
}
