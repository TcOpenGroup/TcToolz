using System;
using System.Diagnostics;
using System.Windows;

namespace Libs.Other
{
    public class Cmd
    {
        public static bool RunCommand(string command)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/C " + command;
                p.StartInfo.CreateNoWindow = true;

                p.Start();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
