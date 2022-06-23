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

namespace HmiPublisher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string GetTitle()
        {
            return "HMI Publisher";
        }

        public static string GetVersion()
        {
            var ver = new Version(Utils.GetVersion());
            return ver.Major + "." + ver.Minor;
        }

        public App()
        {
            //Process curr = Process.GetCurrentProcess();
            //Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            //foreach (Process p in procs)
            //{
            //    if ((p.Id != curr.Id) && (p.MainModule.FileName == curr.MainModule.FileName))
            //    {
            //        Environment.Exit(0);
            //    }
            //}
        }
    }
}
