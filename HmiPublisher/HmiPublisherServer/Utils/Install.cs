/*********************************************************
   Libs\Utils\Install.cs
   
   Copyright (©) 2018 Marek Gvora
*********************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Libs.Utils
{
   public static class Install
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="containsStr"></param>
        /// <returns></returns>
        public static bool IsActiveProcess(string containsStr)
        {
            var allProcesses = Process.GetProcesses();
            foreach (var proc in allProcesses)
            {
                string pr = Convert.ToString(proc);
                if (pr.Contains(containsStr))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool InstallPackage(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("THIS INSTALL PACKAGE DOES NOT EXIST !" + "\n" + filePath, "Beckhoff Installer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            Process process = new Process();
            process.StartInfo.FileName = filePath;
            process.StartInfo.Arguments = "/q";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.Start();
            if (filePath.Contains("TC3"))
            {
                Thread.Sleep(2000);
                while (IsActiveProcess("TC31"));
            }
            process.WaitForExit();
            return true;   
        }
    }
}
