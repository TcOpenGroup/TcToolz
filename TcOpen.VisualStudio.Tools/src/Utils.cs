using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcOTools
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
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    }
}
