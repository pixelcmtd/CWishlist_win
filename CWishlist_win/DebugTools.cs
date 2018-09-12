using SevenZip;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

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

        void button1_Click(object sender, EventArgs e)
        {
            IO.cwll_save(src_form.wl, "test.cwll");
        }

        void button2_Click(object sender, EventArgs e)
        {
            src_form.wl = IO.cwll_load("test.cwll");
            src_form.current_file = "test.cwll";
            src_form.loaded_wl = src_form.wl;
        }

        void button3_Click(object sender, EventArgs e)
        {
            src_form.update_ui();
        }

        void button4_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("test.uncompressed_cwll", FileMode.Create, FileAccess.Write);
            foreach (Item i in src_form.wl)
                i.write_bytes(fs, "L1");
            fs.Close();
        }

        void button5_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("test.deflated_cwll", FileMode.Create, FileAccess.Write);
            fs.Write(Consts.cwll_header, 0, 4);
            fs.write(255);
            DeflateStream ds = new DeflateStream(fs, CompressionLevel.Optimal, false);
            foreach (Item i in src_form.wl)
                i.write_bytes(ds, "L1");
            ds.Close();
        }

        void button6_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("test.cwll_withoutbase64decode", FileMode.Create, FileAccess.Write);
            fs.Write(Consts.cwll_header, 0, 4);
            fs.write(1);
            MemoryStream ms = new MemoryStream();
            foreach (Item i in src_form.wl)
                i.write_bytes(ms, "L1WITHOUTBASE64DECODE");
            SevenZipHelper.Compress(ms, fs);
            ms.Close();
            fs.Close();
        }

        void button7_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("test.uncompressed_cwll_withoutbase64decode", FileMode.Create, FileAccess.Write);
            foreach (Item i in src_form.wl)
                i.write_bytes(fs, "L1WITHOUTBASE64DECODE");
            fs.Close();
        }

        void button8_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("test.uncompressed_cwld", FileMode.Create, FileAccess.Write);
            foreach (Item i in src_form.wl)
                i.write_bytes(fs, "D2");
            fs.Close();
        }
    }
}
