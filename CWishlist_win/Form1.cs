using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace CWishlist_win
{
    public partial class Form1 : Form
    {
        public PluginManager plugin_manager = new PluginManager();
        public WL wl;
        public string current_file = "";
        public string[] recents = new string[0];
        public const string appdata = Registry.CurrentUser.OpenSubKey("Volatile Environment", false).GetValue("APPDATA").ToString();
        public const string appdir = Registry.CurrentUser.OpenSubKey("Volatile Environment", false).GetValue("APPDATA").ToString() + "\\CWishlist";
        public const string plugin_dir = Registry.CurrentUser.OpenSubKey("Volatile Environment", false).GetValue("APPDATA").ToString() + "\\CWishlist\\plugins";
		public const string str_ver = "0x0";
		
        public Form1()
        {
            InitializeComponent();

            if (Program.args.Length > 0)
            {
                wl = IO.load(Program.args[0]);
                current_file = Program.args[0];
            }
            else
                wl = WL.New;

            if (!Directory.Exists(appdir))
                Directory.CreateDirectory(appdir);

            if (!Directory.Exists(plugin_dir))
                Directory.CreateDirectory(plugin_dir);



            if (File.Exists(appdir + "\\recent.cwls"))
                load_recent();
            else
                write_recent();

            if (!File.Exists(appdir + "\\RESTORE_BACKUP"))
                File.WriteAllBytes(appdir + "\\RESTORE_BACKUP", new byte[] { 0x00 });
            else if (File.ReadAllBytes(appdir + "\\RESTORE_BACKUP")[0] == 0x01)
                if (MessageBox.Show(LanguageProvider.get_translated("prompt.restore_backup"), LanguageProvider.get_translated("caption.restore_backup"), MessageBoxButtons.YesNo) == DialogResult.Yes)
                    wl = IO.load(appdir + "\\backup.cwl");

            if (File.Exists(appdir + "\\LANG"))
                LanguageProvider.selected = LanguageProvider.get_lang(File.ReadAllBytes(appdir + "\\LANG")[0]);

            if (File.Exists(appdir + "\\WIDTH"))
                Width = BitConverter.ToInt32(File.ReadAllBytes(appdir + "\\WIDTH"), 0);
            if (File.Exists(appdir + "\\HEIGHT"))
                Height = BitConverter.ToInt32(File.ReadAllBytes(appdir + "\\HEIGHT"), 0);

            if (File.Exists(appdir + "\\COLOR"))
                set_color(BitConverter.ToInt32(File.ReadAllBytes(appdir + "\\COLOR"), 0));

            update_ui();
        }

        void update_ui()
        {
            recentToolStripMenuItem.DropDownItems.Clear();
            if (recents.Length > 0)
                foreach (string r in recents)
                {
                    ToolStripMenuItem itm = new ToolStripMenuItem(r);
                    itm.Click += new EventHandler((sender, e) =>
                    {
                        wl = IO.load(r);
                        current_file = r;
                        update_ui();
                    });
                    recentToolStripMenuItem.DropDownItems.Add(itm);
                }
            else
                recentToolStripMenuItem.DropDownItems.Add(new ToolStripMenuItem("N/A"));
            for (int i = 0; i < wl; i++)
                if (!wl[i].url.StartsWith("http://tinyurl.com") && IO.valid_url(wl[i].url))
                    wl.items[i].url = IO.tinyurl_create(wl[i].url);
            int index = listBox1.SelectedIndex;
            listBox1.Items.Clear();
            foreach(Item i in wl.items)
                listBox1.Items.Add(i.ToString());
            textBox1.Visible = false;
            textBox2.Visible = false;
            label1.Visible = false;
            label2.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            IO.save(wl, appdir + "\\backup.cwl");
            File.WriteAllBytes(appdir + "\\RESTORE_BACKUP", new byte[] { 0x01 });
            listBox1.SelectedIndex = index;
            if (stack_size > 800000)
                MessageBox.Show(LanguageProvider.GetTranslated("prompt.stackoverflow"), LanguageProvider.GetTranslated("caption.stackoverflow"));
            Invalidate();
            Update();
            GC.Collect();
        }

        void write_recent()
        {
            string recentFile = appdir + "\\recent.cwls";
            string tempVer = Path.GetTempFileName();
            string tempRec = Path.GetTempFileName();
            StringBuilder sb = new StringBuilder("<rc>");
            foreach (string r in recents)
                sb.Append(string.Format("<r f=\"{0}\" />", Convert.ToBase64String(Encoding.UTF32.GetBytes(r))));
            sb.Append("</rc>");
            File.WriteAllText(tempRec, sb.ToString());
            File.WriteAllBytes(tempVer, new byte[] { 0x01 });
            if (File.Exists(recentFile))
                File.Delete(recentFile);
            ZipArchive zip = ZipFile.Open(recentFile, ZipArchiveMode.Create, Encoding.ASCII);
            zip.CreateEntryFromFile(tempVer, "V", CompressionLevel.Fastest);
            zip.CreateEntryFromFile(tempRec, "R", CompressionLevel.Optimal);
            zip.Dispose();
        }

        void load_recent()
        {
            string recentFile = appdir + "\\recent.cwls";
            string tempVer = Path.GetTempFileName();
            string tempRec = Path.GetTempFileName();
            ZipArchive zip = ZipFile.Open(recentFile, ZipArchiveMode.Read, Encoding.ASCII);
            zip.GetEntry("V").ExtractToFile(tempVer, true);
            if (File.ReadAllBytes(tempVer)[0] > 0x01)
                throw new TooNewRecentsFileException();
            zip.GetEntry("R").ExtractToFile(tempRec, true);
            zip.Dispose();
            XmlReader xml = XmlReader.Create(tempRec);
            List<string> rcwls = new List<string>();
            while (xml.Read())
                if (xml.NodeType == XmlNodeType.Element && xml.Name == "r")
                    rcwls.Add(Encoding.UTF32.GetString(Convert.FromBase64String(xml.GetAttribute("f"))));
            xml.Close();
            xml.Dispose();
            recents = rcwls.ToArray();
        }

        void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
			bool f = listBox1.SelectedIndex != -1;
            textBox1.Visible = f;
            textBox2.Visible = f;
            label1.Visible = f;
            label2.Visible = f;
            button4.Visible = f;
            button5.Visible = f;
            button6.Visible = f;
			if (f)
			{
				textBox1.Text = wl.items[listBox1.SelectedIndex].name;
                textBox2.Text = wl.items[listBox1.SelectedIndex].url;
			}
            Invalidate();
        }

        void button3_Click(object sender, EventArgs e)
        {
            Item[] old = wl.items;
            wl.items = new Item[old.Length+1];
            for (int i = 0; i < old.Length; i++)
                wl.items[i] = old[i];
            wl.items[old.Length] = new Item("", "");
            update_ui();
        }

        void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            wl.items[listBox1.SelectedIndex].name = textBox1.Text;
            listBox1.Items[listBox1.SelectedIndex] = wl.items[listBox1.SelectedIndex].ToString();
            textBox1.Focus();
            textBox1.SelectionStart = textBox1.TextLength;
            textBox1.SelectionLength = 0;
        }

        void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            wl.items[listBox1.SelectedIndex].url = textBox2.Text;
            Invalidate();
        }

        void button4_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
                for (uint i = 0; i < uint.MaxValue; i++)
                    try
                    {
                        textBox1.Text = Clipboard.GetText();
                        break;
                    }
                    catch { }
        }

        void button5_Click(object sender, EventArgs e)
        {
            if(Clipboard.ContainsText())
                for(uint i = 0; i < uint.MaxValue; i++)
                    try
                    {
                        textBox2.Text = Clipboard.GetText();
                        break;
                    }
                    catch { }
        }

        void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            var url = wl.items[listBox1.SelectedIndex].url;
            Process.Start(url.StartsWith("http") ? url : "http://" + url);
        }

        void button7_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            Item[] old = wl.items;
            wl.items = new Item[old.Length - 1];
            for (int i = 0; i < listBox1.SelectedIndex; i++)
                wl.items[i] = old[i];
            for (int i = listBox1.SelectedIndex + 1; i < old.Length; i++)
                wl.items[i - 1] = old[i];
            try
            {
                update_ui();
            }
            catch
            {
                listBox1.SelectedIndex = wl.Length - 1;
            }
        }

        void Form1_SizeChanged(object sender, EventArgs e)
        {
			//yea, using a var should be faster than using a getter
            int w = Width, h = Height;
            //default window: 427;508
            button1.Location = new Point(w - 271, h / 2 - 16 - 33);
            button2.Location = new Point(w - 271, h / 2 - 10);
            listBox1.Size = new Size(w - 289, h - 93);
            button3.Location = new Point(w - 271, h - 103);
            button4.Location = new Point(w - 72, 26);
            button5.Location = new Point(w - 72, 46);
            button6.Location = new Point(w - 267, 67);
            button7.Location = new Point(w - 271, h - 74);
            button8.Location = new Point(w - 82, h - 74);
            button9.Location = new Point(w - 271, h - 132);
            textBox1.Location = new Point(w - 219, 26);
            textBox2.Location = new Point(w - 219, 46);
            textBox3.Location = new Point(12, h - 71);
            textBox3.Size = new Size(w - 289, 20);
            label1.Location = new Point(w - 271, 24);
            label2.Location = new Point(w - 271, 44);
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (wl != 0 && (current_file == "" || wl != IO.load(current_file)))
                e.Cancel = MessageBox.Show(LanguageProvider.get_translated("prompt.close"), LanguageProvider.get_translated("caption.close"), MessageBoxButtons.YesNo) == DialogResult.No;
            if(!e.Cancel)
            {
                write_recent();
                File.WriteAllBytes(appdir + "\\RESTORE_BACKUP", new byte[] { 0x00 });
                File.WriteAllBytes(appdir + "\\LANG", new byte[] { LanguageProvider.get_id(LanguageProvider.selected) });
                File.WriteAllBytes(appdir + "\\WIDTH", BitConverter.GetBytes(Width));
                File.WriteAllBytes(appdir + "\\HEIGHT", BitConverter.GetBytes(Height));
                File.WriteAllBytes(appdir + "\\COLOR", BitConverter.GetBytes(BackColor.ToArgb()));
            }
        }

        void add_recent_item(string file)
        {
            if (recents.Length != 0 && recents[0] == file)
                return;
            string[] old = recents;
            recents = new string[old.Length + 1];
            recents[0] = file;
            for (int i = 0; i < old.Length; i++)
                recents[i + 1] = old[i];
        }

        void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (wl != 0 && (current_file == "" || wl != IO.load(current_file)))
                if (MessageBox.Show(LanguageProvider.GetTranslated("prompt.new"), LanguageProvider.GetTranslated("caption.new"), MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            if (current_file != "")
                add_recent_item(current_file);
            wl = WL.New;
            current_file = "";
            try
            {
                update_ui();
            }
            catch { }
        }

        void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((current_file == "" && wl != 0) || wl != IO.load(current_file)))
                if (MessageBox.Show(LanguageProvider.GetTranslated("prompt.open"), LanguageProvider.GetTranslated("caption.open"), MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "CWishlists|*.cwl;*.cwlb;*.cwlu",
                Title = "Load CWishlist",
                ValidateNames = true,
                Multiselect = false
            };
            var res = ofd.ShowDialog();
            if (res == DialogResult.Yes || res == DialogResult.OK)
            {
                if (current_file != "")
                    add_recent_item(current_file);
                wl = IO.load(ofd.FileName);
                current_file = ofd.FileName;
                try
                {
                    update_ui();
                }
                catch { }
            }
        }

        void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(current_file == "")
            {
                saveAsToolStripMenuItem_Click(sender, e);
                return;
            }
            if (current_file[current_file.Length - 1] != 'u')
                if (current_file[current_file.Length - 1] == 'l')
                    current_file += 'u';
                else
                {
                    char[] chrs;
                    (chrs = current_file.ToCharArray())[current_file.Length - 1] = 'u';
                    current_file = new string(chrs);
                }
            IO.save(wl, current_file);
        }

        void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                AddExtension = true,
                ValidateNames = true,
                CheckPathExists = true,
                Filter = "CWishlistUncde v1|*.cwlu",
                Title = "Save CWishlist"
            };
            var res = sfd.ShowDialog();
            if (res == DialogResult.Yes || res == DialogResult.OK)
            {
                add_recent_item(sfd.FileName);
                current_file = sfd.FileName;
                IO.save(wl, current_file);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
                saveToolStripMenuItem_Click(keyData, null);
            else if (keyData == (Keys.Control | Keys.Shift | Keys.S))
                saveAsToolStripMenuItem_Click(keyData, null);
            else if (keyData == (Keys.Control | Keys.O))
                openToolStripMenuItem_Click(keyData, null);
            else if (keyData == (Keys.Control | Keys.N))
                newToolStripMenuItem_Click(keyData, null);
            else if (keyData == Keys.Up && listBox1.SelectedIndex > -1)
                listBox1.SelectedIndex--;
            else if (keyData == Keys.Down && listBox1.SelectedIndex + 1 < listBox1.Items.Count)
                listBox1.SelectedIndex++;
            else
                return base.ProcessCmdKey(ref msg, keyData);
            return true;
        }

        void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1 || listBox1.SelectedIndex == 0)
                return;
            Item[] old = wl.items;
            Item[] nev = new Item[old.Length];
            int index = listBox1.SelectedIndex;
            for (int i = 0; i < index - 1; i++)
                nev[i] = old[i];
            nev[index - 1] = old[index];
            nev[index] = old[index - 1];
            for (int i = index + 1; i < old.Length; i++)
                nev[i] = old[i];
            wl.items = nev;
            update_ui();
            listBox1.SelectedIndex = index - 1;
        }

        void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1 || listBox1.SelectedIndex == listBox1.Items.Count - 1)
                return;
            Item[] old = wl.items;
            Item[] nw = new Item[old.Length];
            int index = listBox1.SelectedIndex;
            for (int i = 0; i < index; i++)
                nw[i] = old[i];
            nw[index] = old[index + 1];
            nw[index + 1] = old[index];
            for (int i = index + 2; i < old.Length; i++)
                nw[i] = old[i];
            wl.items = nw;
            update_ui();
            listBox1.SelectedIndex = index + 1;
        }

        void button8_Click(object sender, EventArgs e)
        {
            foreach(Item i in wl)
                Process.Start(i.url.StartsWith("http") ? i.url : "http://" + i.url);
        }

        void changelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tmpfile = Path.GetTempFileName();
            File.WriteAllLines(tmpfile, LanguageProvider.get_translated("misc.changelog"));
            Process.Start("notepad", tmpfile);
        }

        void versionToolStripMenuItem_Click(object sender, EventArgs e) => Process.Start("https://github.com/chrissxYT/CWishlist_win");

        void button9_Click(object sender, EventArgs e)
        {
            wl.items = MergeSorting.MergeSort(wl.items);
            update_ui();
        }

        void languageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(LanguageProvider.GetTranslated("prompt.switch_lang"), LanguageProvider.GetTranslated("caption.switch_lang"), MessageBoxButtons.YesNo) == DialogResult.Yes)
                LanguageProvider.Selected = LanguageProvider.Selected == Language.English ? Language.German : Language.English;
        }

        void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("N/A, ONLY 4 DEBUG!");
        }

        void textBox3_TextChanged(object sender, EventArgs e)
        {
            int i = wl.GetFirstIndex((it) => it.name.ToLower().Contains(textBox3.Text));
            if (i != -1)
                listBox1.SelectedIndex = i;
            textBox3.Focus();
            textBox3.SelectionStart = textBox3.TextLength;
            textBox3.SelectionLength = 0;
        }

        void textBox3_Click(object sender, EventArgs e)
        {
            textBox3.Focus();
            textBox3.SelectionStart = 0;
            textBox3.SelectionLength = textBox3.TextLength;
        }

        uint stack_size
        {
            get
            {
                uint stacksize = 0;
                foreach (Item i in wl.items)
                    stacksize += i;
                return stacksize;
            }
        }

        void set_color(byte r, byte g, byte b) => set_color(Color.FromArgb(r, g, b));

        void set_color(int argb) => set_color(Color.FromArgb(argb));
		
		void set_color(Color c)
		{
			BackColor = c;
            listBox1.BackColor = c;
            textBox1.BackColor = c;
            textBox2.BackColor = c;
            textBox3.BackColor = c;
            button1.BackColor = c;
            button2.BackColor = c;
            button3.BackColor = c;
            button4.BackColor = c;
            button5.BackColor = c;
            button6.BackColor = c;
            button7.BackColor = c;
            button8.BackColor = c;
            button9.BackColor = c;
            menuStrip1.BackColor = c;
		}

        void styleBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string hex = Interaction.InputBox("Please enter a hex value:", "background color hex", "FFFFFF");
                set_color(Convert.ToByte(hex.Substring(0, 2), 16), Convert.ToByte(hex.Substring(2, 2), 16), Convert.ToByte(hex.Substring(4, 2), 16));
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString());
            }
        }
        
        void openPluginDirToolStripMenuItem_Click(object sender, EventArgs e) => Process.Start("explorer", plugin_dir);

        void Form1_Paint(object sender, PaintEventArgs e) => plugin_manager.call_paint_listeners(e);
    }
}
