using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static CWishlist_win.CLinq;
using static CWishlist_win.LanguageProvider;
using static CWishlist_win.Properties.Resources;
using static System.BitConverter;
using static CWishlist_win.Program;
using static System.Text.Encoding;
using static System.Convert;
using static System.Diagnostics.Process;
using static CWishlist_win.IO;

namespace CWishlist_win
{
    public partial class Form1 : Form
    {
        public PluginManager plugin_manager { get; } = new PluginManager();
        public WL wl;
        public string current_file = "";
        public WL loaded_wl = WL.NEW;
        public string[] recents = new string[0];
        public string appdata { get; } = Program.appdata;
        public string appdir { get; } = Program.appdata + "\\CWishlist";
        public string plugin_dir { get; } = Program.appdata + "\\CWishlist\\plugins";
		public string lang_dir { get; } = Program.appdata + "\\CWishlist\\langs";
        public string ver_str = "6.1.0";
        public uint ver_int = 610;
        public byte[] version = new byte[] { 6, 1, 0 };

        public Form1()
        {
            InitializeComponent();
            
            form = this;

            if (args.Length > 0)
                load_wl(args[0]);
            else
                wl = WL.NEW;

            if (!Directory.Exists(appdir))
                Directory.CreateDirectory(appdir);

            if (!Directory.Exists(plugin_dir))
                Directory.CreateDirectory(plugin_dir);

            if (!Directory.Exists(lang_dir))
                Directory.CreateDirectory(lang_dir);
            
            if (!File.Exists(lang_dir + "\\de.xml") || !arrequ(UTF8.GetBytes(de_lang_xml), File.ReadAllBytes(lang_dir + "\\de.xml")))
                File.WriteAllText(lang_dir + "\\de.xml", de_lang_xml);

            if (!File.Exists(lang_dir + "\\en.xml") || !arrequ(UTF8.GetBytes(en_lang_xml), File.ReadAllBytes(lang_dir + "\\en.xml")))
                File.WriteAllText(lang_dir + "\\en.xml", en_lang_xml);

            foreach (string f in Directory.GetFiles(lang_dir))
                load_lang_xml(f);

            if (File.Exists(appdir + "\\recent.cwls"))
                try
                {
                    recents = load_recent(appdir + "\\recent.cwls");
                }
                catch
                {
                    write_recent(appdir + "\\recent.cwls", recents);
                }
            else
                write_recent(appdir + "\\recent.cwls", recents);

            if (!File.Exists(appdir + "\\RESTORE_BACKUP"))
                File.WriteAllBytes(appdir + "\\RESTORE_BACKUP", new byte[] { 0x00 });
            else if (File.ReadAllBytes(appdir + "\\RESTORE_BACKUP")[0] == 0x01 && MessageBox.Show(get_translated("prompt.restore_backup"), get_translated("caption.restore_backup"), MessageBoxButtons.YesNo) == DialogResult.Yes)
                wl = backup_load(appdir + "\\backup.cwl");
            
            if (File.Exists(appdir + "\\LANG"))
            {
                byte[] lfi = File.ReadAllBytes(appdir + "\\LANG");
                if (lfi.Length == 1)
                    selected = get_lang(lfi[0] == 0 ? "en" : "de");
                else
                    selected = get_lang(ASCII.GetString(lfi));
            }

            if (File.Exists(appdir + "\\WIDTH"))
                Width = ToInt32(File.ReadAllBytes(appdir + "\\WIDTH"), 0);

            if (File.Exists(appdir + "\\HEIGHT"))
                Height = ToInt32(File.ReadAllBytes(appdir + "\\HEIGHT"), 0);

            if (File.Exists(appdir + "\\COLOR"))
                set_color(ToInt32(File.ReadAllBytes(appdir + "\\COLOR"), 0));

            //NOPE, THIS SHOULDNT BE ENABLED AT THIS POINT
            if (false)
            {
                foreach (string file in Directory.GetFiles(plugin_dir, "*.cwlwnplg"))
                    try
                    {
                        plugin_manager.load_plugins(file);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), string.Format(get_translated("caption.pluginloadfailed"), file));
                    }

                plugin_manager.call_form_construct_listeners(this);
            }

            update_ui();
        }

        void update_ui()
        {
            GC.TryStartNoGCRegion(15 * 1024 * 1024 + 1024 * 1024 * 127, 127 * 1024 * 1024, true); //no GC for 15MiB of small object heap and 127MiB for big object heap
            recentToolStripMenuItem.DropDownItems.Clear();
            if (recents.Length > 0)
                foreach (string r in recents)
                {
                    ToolStripMenuItem itm = new ToolStripMenuItem(r);
                    itm.Click += new EventHandler((sender, e) =>
                    {
                        load_wl(r);
                        update_ui();
                    });
                    recentToolStripMenuItem.DropDownItems.Add(itm);
                }
            else
                recentToolStripMenuItem.DropDownItems.Add(new ToolStripMenuItem("N/A"));
            for (long i = 0; i < wl.LongLength; i++)
                if (!wl[i].url.StartsWith("http://tinyurl.com/") && wl[i].url.Length > 25 && valid_url(wl[i].url))
                    wl.items[i].url = tinyurl_create(wl[i].url);
            int index = listBox1.SelectedIndex;
            listBox1.Items.Clear();
            foreach(Item i in wl.items)
                listBox1.Items.Add(i.ToString());
            textBox1.Visible = textBox2.Visible = label1.Visible = label2.Visible = button4.Visible = false;
            button5.Visible = button6.Visible = false;
            backup_save(wl, appdir + "\\backup.cwl");
            File.WriteAllBytes(appdir + "\\RESTORE_BACKUP", new byte[] { 1 });
            listBox1.SelectedIndex = index;
            if (stack_size > 800000)
                MessageBox.Show(get_translated("prompt.stackoverflow"), get_translated("caption.stackoverflow"));
            Invalidate();
            Update();
            GC.EndNoGCRegion();
            GC.Collect(2, GCCollectionMode.Forced, false, false);
        }

        void lstbx_index_change(object sender, EventArgs e)
        {
			bool f = listBox1.SelectedIndex != -1;
            textBox1.Visible = textBox2.Visible = label1.Visible = label2.Visible = button4.Visible = f;
            button5.Visible = button6.Visible = f;
			if (f)
			{
				textBox1.Text = wl.items[listBox1.SelectedIndex].name;
                textBox2.Text = wl.items[listBox1.SelectedIndex].url;
			}
        }

        void btn3_click(object sender, EventArgs e)
        {
            Item[] old = wl.items;
            wl.items = new Item[old.Length+1];
            for (int i = 0; i < old.Length; i++)
                wl.items[i] = old[i];
            wl.items[old.Length] = new Item("", "");
            update_ui();
        }

        void txtbx1_change(object sender, EventArgs e)
        {
            wl.items[listBox1.SelectedIndex].name = textBox1.Text;
            listBox1.Items[listBox1.SelectedIndex] = wl.items[listBox1.SelectedIndex].ToString();
            textBox1.Focus();
            textBox1.SelectionStart = textBox1.TextLength;
            textBox1.SelectionLength = 0;
        }

        void txtbx2_change(object sender, EventArgs e) => wl.items[listBox1.SelectedIndex].url = textBox2.Text;

        void btn4_click(object sender, EventArgs e)
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

        void btn5_click(object sender, EventArgs e)
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

        void btn6_click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            var url = wl.items[listBox1.SelectedIndex].url;
            Start(url.StartsWith("http") ? url : "http://" + url);
        }

        void btn7_click(object sender, EventArgs e)
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

        void size_change(object sender, EventArgs e)
        {
            int w = Width;
            int h = Height;
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

        void closing(object sender, FormClosingEventArgs e)
        {
            if ((wl != 0 && current_file == "") || (current_file != "" && wl != loaded_wl))
            {
                bool flag = MessageBox.Show(get_translated("prompt.close"), get_translated("caption.close"), MessageBoxButtons.YesNo) == DialogResult.No;
                e.Cancel = flag;
                if (flag)
                    return;
            }
            if (current_file != "")
                add_recent_item(current_file);
            write_recent(appdir + "\\recent.cwls", recents);
            File.WriteAllBytes(appdir + "\\RESTORE_BACKUP", new byte[] { 0x00 });
            File.WriteAllBytes(appdir + "\\LANG", ASCII.GetBytes(selected.code));
            File.WriteAllBytes(appdir + "\\WIDTH", GetBytes(Width));
            File.WriteAllBytes(appdir + "\\HEIGHT", GetBytes(Height));
            File.WriteAllBytes(appdir + "\\COLOR", GetBytes(BackColor.ToArgb()));
        }

        public void add_recent_item(string file)
        {
            if (recents.Length != 0 && recents[0] == file)
                return;
            string[] old = recents;
            recents = new string[old.Length + 1];
            recents[0] = file;
            for (int i = 0; i < old.Length; i++)
                recents[i + 1] = old[i];
        }

        void new_click(object sender, EventArgs e)
        {
            if ((wl != 0 && current_file == "") || (current_file != "" && wl != loaded_wl))
                if (MessageBox.Show(get_translated("prompt.new"), get_translated("caption.new"), MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            if (current_file != "")
            {
                add_recent_item(current_file);
                current_file = "";
            }
            wl = WL.NEW;
            loaded_wl = WL.NEW;
            try
            {
                update_ui();
            }
            catch { }
        }

        void open_click(object sender, EventArgs e)
        {
            if (((current_file == "" && wl.Length != 0) || (current_file != "" && wl != loaded_wl))
                && MessageBox.Show(get_translated("prompt.open"), get_translated("caption.open"), MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "CWishlists|*.cwl;*.cwlb;*.cwlu;*.cwld",
                Title = "Load CWishlist",
                ValidateNames = true,
                Multiselect = false
            };
            DialogResult res = ofd.ShowDialog();
            if (res == DialogResult.Yes || res == DialogResult.OK)
            {
                if (current_file != "")
                    add_recent_item(current_file);
                load_wl(ofd.FileName);
                try
                {
                    update_ui();
                }
                catch { }
            }
        }

        void save_click(object sender, EventArgs e)
        {
            if (current_file == "")
                save_as_click(sender, e);
            else
            {
                int lm1 = current_file.Length - 1;
                if (current_file[lm1] == 'l')
                    current_file += 'd';
                else if (current_file[lm1] != 'd')
                {
                    char[] c = current_file.ToCharArray();
                    c[lm1] = 'd';
                    current_file = new string(c);
                }
                cwld_save(wl, current_file);
            }
        }

        void save_as_click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                AddExtension = true,
                ValidateNames = true,
                CheckPathExists = true,
                Filter = "CWishlistDeflate|*.cwld",
                Title = "Save CWishlist"
            };
            var res = sfd.ShowDialog();
            if (res == DialogResult.Yes || res == DialogResult.OK)
            {
                add_recent_item(sfd.FileName);
                current_file = sfd.FileName;
                cwld_save(wl, current_file);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
                save_click(keyData, null);
            else if (keyData == (Keys.Control | Keys.Shift | Keys.S))
                save_as_click(keyData, null);
            else if (keyData == (Keys.Control | Keys.O))
                open_click(keyData, null);
            else if (keyData == (Keys.Control | Keys.N))
                new_click(keyData, null);
            else if (keyData == Keys.Up && listBox1.SelectedIndex > -1)
                listBox1.SelectedIndex--;
            else if (keyData == Keys.Down && listBox1.SelectedIndex + 1 < listBox1.Items.Count)
                listBox1.SelectedIndex++;
            else
                return base.ProcessCmdKey(ref msg, keyData);
            return true;
        }

        void btn1_click(object sender, EventArgs e)
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

        void btn2_click(object sender, EventArgs e)
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

        void btn8_click(object sender, EventArgs e)
        {
            foreach(Item i in wl)
                Start(i.url.StartsWith("http") ? i.url : "http://" + i.url);
        }

        void chnglg_click(object sender, EventArgs e)
        {
            string tmp = Path.GetTempFileName();
            File.WriteAllLines(tmp, get_translated("misc.changelog"));
            Start("notepad", tmp);
        }

        void version_click(object sender, EventArgs e) => Start("https://github.com/chrissxYT/CWishlist_win");

        void btn9_click(object sender, EventArgs e)
        {
            update_ui();
            long n = stack_size;
            double log = Math.Log(n, 2);
            long space_complexity = (long)((n * (log + 1)) + (n * log));
            if (space_complexity > 800000)
                MessageBox.Show($"The math shows the space complexity of this merge sort is {space_complexity} Bytes and is a bit high for the 1MiB Stack, a backup is saved, be warned!");
            wl.items = Sorting.merge_sort_items(wl.items);
            update_ui();
        }

        void lang_click(object sender, EventArgs e) => new LanguageSelectionDialog(get_translated("title.switch_lang")).ShowDialog();

        void txtbx3_change(object sender, EventArgs e)
        {
            int i = wl.GetFirstIndex((it) => it.name.ToLower().Contains(textBox3.Text.ToLower()));
            if (i != -1)
                listBox1.SelectedIndex = i;
        }

        void txtbx3_click(object sender, EventArgs e)
        {
            textBox3.Focus();
            textBox3.SelectionStart = 0;
            textBox3.SelectionLength = textBox3.TextLength;
        }

        public long stack_size
        {
            get
            {
                long s = 0;
                foreach (Item i in wl)
                    s += i.MemoryLength;
                return s;
            }
        }

        public void set_color(string hex) => set_color(ToByte(hex.Substring(0, 2), 16), ToByte(hex.Substring(2, 2), 16), ToByte(hex.Substring(4, 2), 16));

        public void set_color(byte r, byte g, byte b) => set_color(Color.FromArgb(r, g, b));

        public void set_color(int argb) => set_color(Color.FromArgb(argb));
		
		public void set_color(Color c)
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

        void style_click(object sender, EventArgs e)
        {
            try
            {
                set_color(Interaction.InputBox("Please enter a hex value:", "background color hex", "FFFFFF"));
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString());
            }
        }
        
        void plugindir_click(object sender, EventArgs e) => Start("explorer", plugin_dir);

        void paint(object sender, PaintEventArgs e) => plugin_manager.call_paint_listeners(e, this);

        public void load_wl(string file)
        {
            wl = load(file);
            current_file = file;
            loaded_wl = wl;
        }

        void debugToolupdateuiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            update_ui();
        }
    }
}
