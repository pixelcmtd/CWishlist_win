using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using static CWishlist_win.Consts;
using static binutils.io;

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
                i.write_bytes(fs, L1);
            fs.Close();
        }

        void button5_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("test.deflated_cwll", FileMode.Create, FileAccess.Write);
            fs.Write(cwll_header, 0, 4);
            fs.write(255);
            DeflateStream ds = new DeflateStream(fs, CompressionLevel.Optimal, false);
            foreach (Item i in src_form.wl)
                i.write_bytes(ds, L1);
            ds.Close();
        }

        void button8_Click(object sender, EventArgs e)
        {
            FileStream fs = File.Open("test.uncompressed_cwld", FileMode.Create, FileAccess.Write);
            foreach (Item i in src_form.wl)
                i.write_bytes(fs, D2);
            fs.Close();
        }

        void button6_Click(object sender, EventArgs e)
        {
            bookmarks bms = Chrome.parse(
                Chrome.bookmark_path_from_appdata_local(@"C:\Users\chrissicx\AppData\Local"));
            dbg(bms.dbgstr(true, "", "    "));
        }
    }
}
