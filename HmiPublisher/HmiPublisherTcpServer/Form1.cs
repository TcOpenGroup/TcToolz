using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace HmiPublisherServer
{
    public partial class Form1 : Form
    {
        private readonly ControlInterface _controlInterface;

        private TcpListener _server;

        public Form1()
        {
            InitializeComponent();

            _controlInterface = new ControlInterface();

            TopMost = true;

            richTextBox1.ReadOnly = true;

            richTextBox1.AppendText("Listening ...\n");

            _server = new TcpListener(IPAddress.Parse("0.0.0.0"), 13700);

            //try
            //{
            //    SetStartup();
            //}
            //catch (Exception ex)
            //{
            //    richTextBox1.AppendText(ex.Message + "\n");

            //}
        }

        private void SetStartup()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            rk.SetValue("HmiPublisherServer", Application.ExecutablePath, RegistryValueKind.String);
        }

        private void OnCommandReceived(string key, string[] args)
        {
            if (args.Length > 0)
            {
                AppendText(key + ";" + args[0]);
            }
            else
            {
                AppendText(key);
            }

            switch (key)
            {
                case "system":
                    _controlInterface.StartCmd(args[0]);
                    break;
                case "poweroff":
                    _controlInterface.StartCmd("shutdown -s -t 0");
                    break;
                case "reboot":
                    _controlInterface.StartCmd("shutdown -r -t 0");
                    break;
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
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _server.Start();
            backgroundWorker1.RunWorkerAsync();

            BeginInvoke(new MethodInvoker(delegate
            {
                Hide();
            }));
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            Show();

            this.Location = new Point(GetScreen().Width - this.Size.Width - 5, GetScreen().Height - this.Size.Height - 30 - 5);
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

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
    }
}
