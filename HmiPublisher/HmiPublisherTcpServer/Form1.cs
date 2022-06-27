using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HmiPublisherTcpServer
{
    public partial class Form1 : Form
    {
        const int Port = 13700;

        private TcpListener _server;

        int GetTaskBarHeight()
        {
            int PSH = GetScreen().Height;
            int PSBH = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            double ratio = PSH / PSBH;
            int TaskBarHeight = PSBH - System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            TaskBarHeight *= (int)ratio;
            return TaskBarHeight;
        }


        public Form1()
        {


            InitializeComponent();



            TopMost = true;

            richTextBox1.ReadOnly = true;

            var ver = new Version(Utils.GetVersion());
            var version = ver.Major + "." + ver.Minor;

            richTextBox1.Text = ("HmiPublisher Server [Version " + version + "]\n");


            _server = new TcpListener(IPAddress.Parse("0.0.0.0"), Port);
        }


        private void OnCommandReceived(string key, string[] args)
        {

            BeginInvoke(new MethodInvoker(delegate
            {
                panel1.BackColor = Color.Green;
            }));

            if (args.Length > 0)
            {
                AppendText(key + ";" + args[0]);
            }
            else
            {
                AppendText(key);
            }

            try
            {
                switch (key)
                {
                    case "system":
                        Utils.StartCmd(args[0]);
                        break;
                    case "poweroff":
                        Utils.StartCmd("shutdown -s -t 0");
                        break;
                    case "reboot":
                        Utils.StartCmd("shutdown -r -t 0");
                        break;
                }
            }
            catch (Exception ex)
            {
                AppendText(ex.Message);
                BeginInvoke(new MethodInvoker(delegate
                {
                    panel1.BackColor = Color.Red;
                    ShowBottomRight();
                }));
            }
        }

        void AppendText(string text)
        {
            richTextBox1.Invoke((MethodInvoker)delegate
            {
                richTextBox1.AppendText(text + "\n");
            });
        }

        [Obsolete]
        private void backgroundWorker1_DoWorkAsync(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    var bytes = new byte[256];
                    var data = "";

                    var client = _server.AcceptTcpClient();
                    data = "";
                    var stream = client.GetStream();

                    int i;

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        try
                        {
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i).ToLower();

                            string[] args = data.Split(';');

                            var key = Regex.Replace(args[0], @"\s+", "");

                            if (args.Length > 1)
                            {
                                OnCommandReceived(key, args.Skip(1).ToArray());
                            }
                            else
                            {
                                OnCommandReceived(key, args.Skip(1).ToArray());
                            }


                            bool number = true;
                            byte[] msg = BitConverter.GetBytes(number);
                            msg = new byte[] { 1 };
                            stream.Write(msg, 0, msg.Length);
                        }
                        catch (Exception ex)
                        {
                            AppendText(ex.Message);
                        }
                    }
                    client.Close();
                }
                catch (Exception ex)
                {
                    AppendText(ex.Message);
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        panel1.BackColor = Color.Red;
                        ShowBottomRight();
                    }));
                }
            }

        }

        bool IsPortOpen(string host, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne();
                    client.EndConnect(result);
                    return success;
                }
            }
            catch
            {
                return false;
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            BeginInvoke(new MethodInvoker(delegate
            {
                Hide();
            }));

            try
            {
                _server.Start();

                AppendText($"Listening on port {Port}");

                backgroundWorker1.RunWorkerAsync();

            }
            catch (Exception ex)
            {
                AppendText(ex.Message);
                BeginInvoke(new MethodInvoker(delegate
                {
                    panel1.BackColor = Color.Red;
                    ShowBottomRight();
                }));
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                ShowBottomRight();
            }
        }

        private void ShowBottomRight()
        {
            BeginInvoke(new MethodInvoker(delegate
            {
                Show();
            }));
            this.Location = new Point(GetScreen().Width - this.Size.Width - 5, GetScreen().Height - GetTaskBarHeight() - this.Size.Height - 5);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        public Rectangle GetScreen()
        {
            return Screen.FromControl(this).Bounds;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                var res = MessageBox.Show("Do want to close HMI PUBLISHER SERVER?", "HMI PUBLISHER SERVER", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    this.Close();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }


    }
}
