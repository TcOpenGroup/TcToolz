using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = command;
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
