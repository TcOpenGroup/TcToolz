using System.Diagnostics;
using System.Windows.Forms;

namespace HmiPublisherServer
{
    public class ControlInterface
    {
        public void StartCmd(string cmd)
        {
            try
            {
                var proc = new ProcessStartInfo();
                proc.FileName = @"C:\windows\system32\cmd.exe";
                proc.CreateNoWindow = true;
                proc.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Arguments = $"/c {cmd}";
                var p = Process.Start(proc);
                p.WaitForExit();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
