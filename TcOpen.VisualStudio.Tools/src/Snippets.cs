using EnvDTE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcOTools
{
    public partial class Snippets : Form
    {
        string _filePath = "";
        List<string> _items = new List<string>();
        TcODTE _dte;

        public Snippets(TcODTE dte)
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;
            ShowIcon = false;
            ShowInTaskbar = false;
            MinimizeBox = false;

            textBox1.KeyDown += TextBox1_KeyDown;
            textBox1.TextChanged += TextBox1_TextChanged; ;

            listBox1.Enter += ListBox1_Enter;
            listBox1.KeyDown += ListBox1_KeyDown;
            listBox1.MouseDoubleClick += ListBox1_MouseDoubleClick;

            _dte = dte;
            _filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TcOSnippets";

            Reload();
        }



        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (listBox1.Items.Count > 0)
                    {
                        var text = listBox1.Items[0].ToString();
                        Hide();
                        await PasteAsync(text);
                        this.Close();
                    }
                }
                if (e.KeyCode == Keys.Down)
                {
                    if (listBox1.Items.Count > 0)
                    {
                        listBox1.Focus();
                        listBox1.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                listBox1.Items.Clear();

                foreach (string str in _items)
                {

                    if (str.ToLower().Contains(textBox1.Text.ToLower()))
                    {
                        listBox1.Items.Add(str);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ListBox1_Enter(object sender, EventArgs e)
        {
            try
            {
                listBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void ListBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    var text = listBox1.SelectedItems[0].ToString();
                    Hide();
                    await PasteAsync(text);
                    this.Close();
                }
                if (e.KeyCode == Keys.Up)
                {
                    if (listBox1.SelectedIndex == 0)
                    {
                        textBox1.Focus();

                    }
                }
                if (e.KeyCode == Keys.F12)
                {
                    var text = listBox1.SelectedItems[0].ToString();
                    using (var sr = new StreamReader(_filePath + @"\" + text + ".txt"))
                    {
                        MessageBox.Show(sr.ReadToEnd(), "Lookup", MessageBoxButtons.OK, MessageBoxIcon.None);
                    }
                }
                if (e.KeyCode == Keys.F2)
                {
                    textBox1.Focus();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void ListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (listBox1.Items.Count > 0)
                {
                    var text = listBox1.SelectedItems[0].ToString();
                    Hide();
                    await PasteAsync(text);
                    this.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private async Task PasteAsync(string text)
        {
            await Task.Factory.StartNew(() =>
            {
                using (var sr = new StreamReader(_filePath + @"\" + text + ".txt"))
                {
                    var str = sr.ReadToEnd();

                    System.Threading.Thread.Sleep(10);


                    var backupClipboard = Utils.GetClipboardText();

                    if (!string.IsNullOrEmpty(str))
                    {
                        Utils.SetClipboardText(str);
                    }

                    _dte.ExecuteCommand(Command.Paste);

                    if (!string.IsNullOrEmpty(backupClipboard))
                    {
                        Utils.SetClipboardText(backupClipboard);
                    }

                }
            });
        }

        private void NewSnippetButton_Click(object sender, EventArgs e)
        {
            var form = new NewSnippet(_filePath);
            form.ShowDialog();
            Reload();
        }

        private void OpenContainingFolderButton_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", _filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            Reload();
        }


        private void Reload()
        {
            try
            {
                if (!Directory.Exists(_filePath))
                {
                    Directory.CreateDirectory(_filePath);
                }
                _items.Clear();
                listBox1.Items.Clear();
                DirectoryInfo d = new DirectoryInfo(_filePath);
                FileInfo[] Files = d.GetFiles("*.txt", SearchOption.AllDirectories);
                foreach (FileInfo file in Files)
                {
                    _items.Add(Path.GetFileNameWithoutExtension(file.Name));
                }

                foreach (string str in _items)
                {
                    listBox1.Items.Add(str);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
