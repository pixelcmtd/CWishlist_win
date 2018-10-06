using Microsoft.VisualBasic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static CWishlist_win.CLinq;
using static CWishlist_win.LanguageProvider;
using static CWishlist_win.Properties.Resources;
using static CWishlist_win.Program;
using static System.Diagnostics.Process;
using static CWishlist_win.IO;
using static System.GC;
using static CWishlist_win.WL;
using static System.Windows.Forms.Clipboard;
using static System.Drawing.Color;
using static CWishlist_win.Sorting;
using static System.Windows.Forms.Keys;
using static System.Windows.Forms.DialogResult;
using static System.Windows.Forms.MessageBoxButtons;
using System.Threading;
using System.Collections.Generic;

namespace CWishlist_win
{
    public partial class Form1 : Form
    {
        public PluginManager plugin_manager { get; } = new PluginManager();
        public WL wl;
        public string current_file = "";
        public WL loaded_wl = NEW;
        public string[] recents = { };
        public readonly string appdir = appdata + "\\CWishlist";
        public readonly string plugin_dir = appdata + "\\CWishlist\\plugins";
		public readonly string lang_dir = appdata + "\\CWishlist\\langs";
        public readonly string lang_de = appdata + "\\CWishlist\\langs\\de.xml";
        public readonly string lang_en = appdata + "\\CWishlist\\langs\\en.xml";
        public readonly string recents_file = appdata + "\\CWishlist\\recent.cwls";
        public readonly string lang_file = appdata + "\\CWishlist\\L";
        public readonly string legacy_lang_file = appdata + "\\CWishlist\\LANG";
        public readonly string width_file = appdata + "\\CWishlist\\W";
        public readonly string legacy_width_file = appdata + "\\CWishlist\\WIDTH";
        public readonly string height_file = appdata + "\\CWishlist\\H";
        public readonly string legacy_height_file = appdata + "\\CWishlist\\HEIGHT";
        public readonly string backup_file = appdata + "\\CWishlist\\backup.cwl";
        public readonly string restore_backup = appdata + "\\CWishlist\\RESTORE_BACKUP";
        public readonly string color_file = appdata + "\\CWishlist\\C";
        public readonly string legacy_color_file = appdata + "\\CWishlist\\COLOR";
        //this isnt even beta at this point, main features arent done and there are many bugs, so its an alpha
        public string ver_str = "7.0.0a"; 
        public uint ver_int = 0x700a;
        public byte[] version = new byte[] { 7, 0, 0, 254 };
        public object recents_mutex = new object();
        public object backup_mutex = new object();
        public object rbackup_mutex = new object();

        public Form1()
        {
            Thread t;
            Thread u;
            Thread v;
            Thread w;
            Thread x;
            Thread y;
            Thread z;

            InitializeComponent();

            if (args.Length > 0)
                load_wl(args[0]);
            else
                wl = NEW;

            (v = new Thread(() =>
            {
                if (!Directory.Exists(appdir))
                    Directory.CreateDirectory(appdir);
            })).Start();

            (x = new Thread(() =>
            {
                if (!Directory.Exists(lang_dir))
                    Directory.CreateDirectory(lang_dir);
            })).Start();

            (w = new Thread(() =>
            {
                if (!Directory.Exists(plugin_dir))
                    Directory.CreateDirectory(plugin_dir);
            })).Start();

            x.Join();

            (t = new Thread(() =>
            {
                if (!File.Exists(lang_de) || !arrequ(utf8(de_lang_xml), File.ReadAllBytes(lang_de)))
                    File.WriteAllText(lang_de, de_lang_xml);
            })).Start();

            (u = new Thread(() =>
            {
                if (!File.Exists(lang_en) || !arrequ(utf8(en_lang_xml), File.ReadAllBytes(lang_en)))
                    File.WriteAllText(lang_en, en_lang_xml);
            })).Start();

            (x = new Thread(() =>
            {
                lock (recents_mutex)
                {
                    if (File.Exists(recents_file))
                        try
                        {
                            lock (recents_mutex)
                            {
                                recents = load_recent(recents_file);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Corrupted recents:\n\n" + e);
                            try
                            {
                                lock (recents_mutex)
                                {
                                    write_recent(recents_file, recents);
                                }
                            }
                            catch (Exception e1)
                            {
                                MessageBox.Show("Unable to write new recents:\n\n" + e1);
                            }
                        }
                    else
                        lock (recents_mutex)
                        {
                            write_recent(recents_file, recents);
                        }
                }
            })).Start();

            t.Join();

            (t = new Thread(() =>
            {
                u.Join();

                foreach (string f in Directory.GetFiles(lang_dir))
                    load_lang_xml(f);

                if (File.Exists(lang_file))
                    selected = get_lang(ascii(File.ReadAllBytes(lang_file)));
                else if (File.Exists(legacy_lang_file))
                {
                    byte[] c = File.ReadAllBytes(legacy_lang_file);
                    if (c.Length == 1)
                        selected = get_lang(c[0] == 0 ? "en" : "de");
                    else
                        selected = get_lang(ascii(c));
                }

                if (!File.Exists(restore_backup))
                    File.WriteAllBytes(restore_backup, new byte[] { 0 });
                else if (File.ReadAllBytes(restore_backup)[0] != 0 &&
                    MessageBox.Show(get_translated("prompt.restore_backup"), get_translated("caption.restore_backup"), YesNo) == Yes)
                    lock (backup_mutex)
                    {
                        wl = backup_load(backup_file);
                    }
            })).Start();

            (y = new Thread(() =>
            {
                if (File.Exists(width_file))
                    Width = int32(File.ReadAllBytes(width_file));
                else if (File.Exists(legacy_width_file))
                {
                    Width = int32(File.ReadAllBytes(legacy_width_file));
                    File.Delete(legacy_width_file);
                }
            })).Start();

            (z = new Thread(() =>
            {
                if (File.Exists(height_file))
                    Height = int32(File.ReadAllBytes(height_file));
                else if (File.Exists(legacy_height_file))
                {
                    Height = int32(File.ReadAllBytes(legacy_height_file));
                    File.Delete(legacy_height_file);
                }
            })).Start();

            u.Join();

            (u = new Thread(() =>
            {
                if (File.Exists(color_file))
                {
                    byte[] b = File.ReadAllBytes(color_file);
                    set_color(b[0], b[1], b[2]);
                }
                else if(File.Exists(legacy_color_file))
                {
                    set_color(int32(File.ReadAllBytes(legacy_color_file)));
                    File.Delete(legacy_color_file);
                }
            })).Start();

#if false
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
#endif

            update_ui();
        }

        public void update_ui()
        {
            //no GC for [up to]15MiB of small object heap and 127MiB for big object heap
            try
            {
                TryStartNoGCRegion(15 * 1024 * 1024 + 1024 * 1024 * 127, 127 * 1024 * 1024, true);
            }
            catch (Exception e)
            {
                MessageBox.Show("Can't start no GC area:\n" + e);
            }
            
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
            List<Thread> t = new List<Thread>();
            foreach (Item i in wl)
                if (!i.url.StartsWith("http://tinyurl.com/") && i.url.Length > 27 && valid_url(i.url))
                    t.Add(start(() => { i.url = tinyurl_create(i.url); }));
            start(() =>
            {
                try
                {
                    lock(backup_mutex)
                    {
                        backup_save(wl, backup_file);
                    }
                }
                catch { }
            });
            start(() =>
            {
                try
                {
                    lock(rbackup_mutex)
                    {
                        File.WriteAllBytes(restore_backup, new byte[] { 1 });
                    }
                }
                catch { }
            });
            int index = listBox1.SelectedIndex;
            listBox1.Items.Clear();
            foreach (Thread tt in t)
                tt.Join();
            foreach (Item i in wl.items)
                listBox1.Items.Add(i.ToString());
            textBox1.Visible = textBox2.Visible = label1.Visible = label2.Visible = button4.Visible = button5.Visible = button6.Visible = false;
            listBox1.SelectedIndex = index;
            Invalidate();
            Update();
            EndNoGCRegion();
            Collect(2, GCCollectionMode.Forced, false, false);
        }

        void lstbx_index_change(object sender, EventArgs e)
        {
			bool f = listBox1.SelectedIndex != -1;
            textBox1.Visible = textBox2.Visible = label1.Visible = label2.Visible = button4.Visible = button5.Visible = button6.Visible = f;
			if (f)
			{
				textBox1.Text = wl.items[listBox1.SelectedIndex].name;
                textBox2.Text = wl.items[listBox1.SelectedIndex].url;
			}
        }

        void btn3_click(object sender, EventArgs e)
        {
            Item[] old = wl.items;
            wl.items = new Item[old.Length + 1];
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

        void txtbx2_change(object sender, EventArgs e)
        {
            wl[listBox1.SelectedIndex].url = textBox2.Text;
        }

        void btn4_click(object sender, EventArgs e)
        {
            if (ContainsText())
                for (uint i = 0; i < uint.MaxValue; i++)
                    try
                    {
                        textBox1.Text = GetText();
                        break;
                    }
                    catch { }
        }

        void btn5_click(object sender, EventArgs e)
        {
            if (ContainsText())
                start(() =>
                {
                    for (uint i = 0; i < uint.MaxValue; i++)
                        try
                        {
                            textBox2.Text = GetText();
                            break;
                        }
                        catch { }
                });
        }

        void btn6_click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            string url = wl.items[listBox1.SelectedIndex].url;
            Start(url.StartsWith("http") ? url : "http://" + url);
        }

        void remove_click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            Item[] old = wl.items;
            wl.items = new Item[old.Length - 1];
            memcpy(old, wl.items, listBox1.SelectedIndex);
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

        void size_change(object _, EventArgs e)
        {
            int w = Width;
            int h = Height;
            int h2 = h / 2;
            button1.Location = new Point(w - 271, h2 - 16 - 33);
            button2.Location = new Point(w - 271, h2 - 10);
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

        void closing(object _, FormClosingEventArgs e)
        {
            if ((wl > 0 && current_file == "") || (current_file != "" && wl != loaded_wl))
            {
                bool flag = MessageBox.Show(get_translated("prompt.close"), get_translated("caption.close"), YesNo) == No;
                e.Cancel = flag;
                if (flag)
                    return;
            }
            if (current_file != "")
                start(() => { lock (recents_mutex) { add_recent_item(current_file); } });
            start(() => { lock (recents_mutex) { write_recent(recents_file, recents); } });
            start(() => File.WriteAllBytes(restore_backup, new byte[] { 0 }));
            start(() => File.WriteAllBytes(lang_file, ascii(selected.code)));
            start(() => File.WriteAllBytes(width_file, bytes(Width)));
            start(() => File.WriteAllBytes(height_file, bytes(Height)));
            start(() =>
            {
                Color c = BackColor;
                File.WriteAllBytes(color_file, new byte[] { c.R, c.G, c.B });
            });
        }

        public void add_recent_item(string file)
        {
            start(() =>
            {
                lock (recents_mutex)
                {
                    if (recents.Length != 0 && recents[0] == file)
                        return;
                    string[] old = recents;
                    recents = new string[old.Length + 1];
                    recents[0] = file;
                    for (int i = 0; i < old.Length; i++)
                        recents[i + 1] = old[i];
                }
            });
        }

        void new_click(object _, EventArgs e)
        {
            if ((wl != 0 && current_file == "") || (current_file != "" && wl != loaded_wl)
                && MessageBox.Show(get_translated("prompt.new"), get_translated("caption.new"), YesNo) == No)
                    return;
            if (current_file != "")
            {
                add_recent_item(current_file);
                current_file = "";
            }
            wl = NEW;
            loaded_wl = NEW;
            try
            {
                update_ui();
            }
            catch { }
        }

        void open_click(object sender, EventArgs e)
        {
            if (((current_file == "" && wl.Length > 0) || (current_file != "" && wl != loaded_wl))
                && MessageBox.Show(get_translated("prompt.open"), get_translated("caption.open"), YesNo) == No)
                return;
            OpenFileDialog ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "CWishlists|*.cwld;*.cwlu;*.cwlb;*.cwl",
                Title = "Load CWishlist",
                ValidateNames = true,
                Multiselect = false
            };
            DialogResult res = ofd.ShowDialog();
            if (res == Yes || res == DialogResult.OK)
            {
                if (current_file != "")
                    add_recent_item(current_file);
                load_wl(ofd.FileName);
            }
        }

        void save_click(object sender, EventArgs e)
        {
            if (current_file == "")
                save_as_click(sender, e);
            else
            {
                int lm1 = current_file.Length - 1;
                int lm2 = current_file.Length - 2;
                if (current_file[lm1] == 'l' && current_file[lm2] == 'w')
                    current_file += 'd';
                else if (current_file[lm1] != 'd')
                {
                    char[] c = current_file.ToCharArray();
                    c[lm1] = 'd';
                    current_file = new string(c);
                }
                update_ui();
                cwld_save(wl, current_file);
            }
        }

        void save_as_click(object sender, EventArgs e)
        {
            update_ui();
            SaveFileDialog sfd = new SaveFileDialog()
            {
                AddExtension = true,
                ValidateNames = true,
                CheckPathExists = true,
                Filter = "CWishlistDeflate|*.cwld",
                Title = "Save CWishlist"
            };
            var res = sfd.ShowDialog();
            if (res == Yes || res == DialogResult.OK)
            {
                add_recent_item(sfd.FileName);
                current_file = sfd.FileName;
                cwld_save(wl, current_file);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | S))
                save_click(keyData, null);
            else if (keyData == (Keys.Control | Shift | S))
                save_as_click(keyData, null);
            else if (keyData == (Keys.Control | O))
                open_click(keyData, null);
            else if (keyData == (Keys.Control | N))
                new_click(keyData, null);
            else if (keyData == Up && listBox1.SelectedIndex != -1)
                listBox1.SelectedIndex--;
            else if (keyData == Down && listBox1.SelectedIndex + 1 < listBox1.Items.Count)
                listBox1.SelectedIndex++;
            else
                return base.ProcessCmdKey(ref msg, keyData);
            update_ui();
            return true;
        }

        void btn1_click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1 || listBox1.SelectedIndex == 0)
                return;
            Item[] old = wl.items;
            wl.items = new Item[old.Length];
            int index = listBox1.SelectedIndex;
            for (int i = 0; i < index - 1; i++)
                wl.items[i] = old[i];
            wl.items[index - 1] = old[index];
            wl.items[index] = old[index - 1];
            for (int i = index + 1; i < old.Length; i++)
                wl.items[i] = old[i];
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
            Start("notepad", $"\"{tmp}\"");
        }

        /// <summary>
        /// Opens the GitHub repo in the default browser.
        /// </summary>
        void version_click(object sender, EventArgs e)
        {
            Start("https://github.com/chrissxYT/CWishlist_win");
        }

        void btn9_click(object sender, EventArgs e)
        {
            update_ui();
            wl.items = merge_sort_items(wl.items);
            update_ui();
        }

        void lang_click(object sender, EventArgs e)
        {
            new LanguageSelectionDialog(get_translated("title.switch_lang")).ShowDialog();
        }

        void txtbx3_change(object sender, EventArgs e)
        {
            int i = wl.GetFirstIndex((it) => it.name.ToLower().Contains(textBox3.Text.ToLower()));
            if (i != -1)
                listBox1.SelectedIndex = i;
        }

        void search_click(object sender, EventArgs e)
        {
            textBox3.Focus();
            textBox3.SelectionStart = 0;
            textBox3.SelectionLength = textBox3.TextLength;
        }

        /// <summary>
        /// Sets all the colors to the given one.
        /// </summary>
        public void set_color(string hex)
        {
            set_color(CLinq.hex(hex.Substring(0, 2)), CLinq.hex(hex.Substring(2, 2)), CLinq.hex(hex.Substring(4, 2)));
        }

        /// <summary>
        /// Sets all the colors to the given one.
        /// </summary>
        public void set_color(byte r, byte g, byte b) => set_color(FromArgb(r, g, b));

        /// <summary>
        /// Sets all the colors to the given one.
        /// </summary>
        public void set_color(int argb) => set_color(FromArgb(argb));
		
        /// <summary>
        /// Sets all the colors to the given one.
        /// </summary>
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

        /// <summary>
        /// Opens an InputBox for the color.
        /// </summary>
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
        
        /// <summary>
        /// Starts the windows explorer in the plugin dir.
        /// </summary>
        void plugindir_click(object sender, EventArgs e)
        {
            Start("explorer", plugin_dir);
        }

        /// <summary>
        /// "Hook" to invoke the paint listeners in the plugin manager.
        /// </summary>
        void paint(object sender, PaintEventArgs e) => plugin_manager.call_paint_listeners(e, this);

        /// <summary>
        /// Loads the CWL from the given file to loaded_wl, wl and current_file.
        /// </summary>
        /// <param name="file">The file to load the CWL from.</param>
        public void load_wl(string file)
        {
            wl = load(file);
            current_file = file;
            loaded_wl = wl;
            try
            {
                update_ui();
            }
            catch { }
        }

        /// <summary>
        /// Shows the DebugTools as a dialog.
        /// </summary>
        void debugToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new DebugTools(this).ShowDialog();
        }
    }
}
