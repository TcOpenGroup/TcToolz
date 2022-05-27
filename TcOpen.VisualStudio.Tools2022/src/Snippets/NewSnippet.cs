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

namespace TcOpen.VisualStudio.Tools
{
    public partial class NewSnippet : Form
    {
        string _filePath = "";
        public NewSnippet(string filePath)
        {
            InitializeComponent();
            CenterToScreen();
            TopMost = true;
            ShowIcon = false;
            ShowInTaskbar = false;
            MinimizeBox = false;

            _filePath = filePath;
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

        private void AddSnippetButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(_filePath))
                {
                    Directory.CreateDirectory(_filePath);
                }
                using (StreamWriter writetext = new StreamWriter(_filePath + @"\" + textBox1.Text + ".txt"))
                {
                    writetext.Write(richTextBox1.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
