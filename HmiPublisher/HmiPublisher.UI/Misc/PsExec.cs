
namespace Libs.Other
{
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;

    public class PsExec
    {
        /// <summary>
        /// -i Interactive - Run the program so that it interacts with the desktop on the remote system. If no session is specified, the process runs in the console session.
        /// -d Don’t wait for the application to terminate.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="userName"></param>
        /// <param name="pass"></param>
        /// <param name="programFilePath"></param>
        /// <returns></returns>
        public static bool RunProgram(string host, string userName, string pass, string programFilePath)
        {
            AcceptEula();
            try
            {
                if (File.Exists("PsExec64.exe"))
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "PsExec64.exe";

                    p.StartInfo.Arguments = @"\\" + host + " -accepteula" + " -u " + userName + " -p " + pass + " -s -i -d " + programFilePath;
                    //p.StartInfo.Arguments = @"\\" + host + " -accepteula" + " -s -i -d " + programFilePath + " -u " + userName + " -p " + pass; // missing
                    p.StartInfo.CreateNoWindow = true;
                    p.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);

                    p.Start();
                    p.WaitForExit();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="userName"></param>
        /// <param name="pass"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool RunCommand(string host, string userName, string pass, string command)
        {
            AcceptEula();
            try
            {
                if (File.Exists("PsExec64.exe"))
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "PsExec64.exe";
                    p.StartInfo.Arguments = @"\\" + host + " -accepteula" + " -u " + userName + " -p " + pass + " -s " + command; //" -s -d "
                    p.StartInfo.CreateNoWindow = true;
                    p.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);

                    p.Start();
                    p.WaitForExit();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private static void AcceptEula()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Sysinternals\PsExec");
                if (key != null)
                {
                    key.SetValue("EulaAccepted", 0x01, RegistryValueKind.DWord);
                    key.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }
        private static void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

        }
    }
}
