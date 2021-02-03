using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using static CWishlist_win.Consts;
using static binutils.io;
using Microsoft.VisualBasic;

namespace CWishlist_win
{
    public partial class DebugTools : Form
    {
        Form1 src_form;

        public DebugTools(Form1 src_frm)
        {
            InitializeComponent();
            src_form = src_frm;
        }

        void button3_Click(object sender, EventArgs e)
        {
            src_form.update_ui();
        }

        void button4_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("test.uncompressed_cwll", FileMode.Create, FileAccess.Write);
            foreach (Item i in src_form.wl)
                i.write_bytes(fs, 3);
            fs.Close();
        }

        void button5_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("test.deflated_cwll", FileMode.Create, FileAccess.Write);
            fs.Write(cwll_header, 0, 4);
            fs.write(255);
            DeflateStream ds = new DeflateStream(fs, CompressionLevel.Optimal, false);
            foreach (Item i in src_form.wl)
                i.write_bytes(ds, 3);
            ds.Close();
        }

        void button8_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("test.uncompressed_cwld", FileMode.Create, FileAccess.Write);
            foreach (Item i in src_form.wl)
                i.write_bytes(fs, 2);
            fs.Close();
        }

        void button6_Click(object sender, EventArgs e)
        {
        }
        
        void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(IO.tinyurl_resolve(Interaction.InputBox("tinyurl:")));
        }

        void button2_Click(object sender, EventArgs e)
        {
        }
    }
}
