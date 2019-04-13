using System;
using System.Windows.Forms;

namespace CWishlist_win
{
    public partial class CLSettings : Form
    {
        Form1 form;

        public CLSettings(Form1 form)
        {
            InitializeComponent();
            this.form = form;
            textBox1.Text = form.cl_exe;
            textBox2.Text = form.cl_args;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = @"C:\Program Files(x86)\Google\Chrome\Application\chrome.exe";
            textBox2.Text = "$URL";
            save_cl();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = @"C:\Program Files\Mozilla Firefox\firefox.exe";
            textBox2.Text = "$URL";
            save_cl();
        }

        void save_cl()
        {
            form.cl_exe  = textBox1.Text;
            form.cl_args = textBox2.Text;
        }

        private void CLSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            save_cl();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            form.cl_exe = textBox1.Text;
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            form.cl_args = textBox2.Text;
        }
    }
}
