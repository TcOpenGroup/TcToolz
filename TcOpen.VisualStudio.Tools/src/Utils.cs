using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TcOpen.VisualStudio.Tools
{
    public class Utils
    {
        public static void AddCommand(Guid cmdSet, int cmdId, Action invoke, IMenuCommandService commandService)
        {
            var command = new OleMenuCommand((s, e) => invoke(), new CommandID(cmdSet, cmdId));
            commandService.AddCommand(command);
        }

        public static void RunWithErrorHandling(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void SetClipboardText(string p_Text)
        {
            Thread STAThread = new Thread(
                delegate ()
                {
                    if (!string.IsNullOrEmpty(p_Text))
                        System.Windows.Forms.Clipboard.SetText(p_Text);
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        public static string GetClipboardText()
        {
            string ReturnValue = string.Empty;
            Thread STAThread = new Thread(
                delegate ()
                {
                    ReturnValue = System.Windows.Forms.Clipboard.GetText();
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

            return ReturnValue;
        }

        public static void RunExternalProgram(string workingDirectory, string program, string arg)
        {
            if (File.Exists(program))
            {
                var startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = workingDirectory;
                startInfo.FileName = program;
                startInfo.Arguments = arg;
                var proc = System.Diagnostics.Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show("The program doesn't exist", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

    }
}
