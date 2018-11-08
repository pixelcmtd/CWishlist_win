using Microsoft.VisualBasic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static CWishlist_win.CLinq;
using static CWishlist_win.Encodings;
using static CWishlist_win.LanguageProvider;
using static CWishlist_win.Properties.Resources;
using static CWishlist_win.Program;
using static CWishlist_win.Consts;
using static System.Diagnostics.Process;
using static CWishlist_win.IO;
using static CWishlist_win.WL;
using static CWishlist_win.Item;
using static System.Windows.Forms.Clipboard;
using static System.Drawing.Color;
using static CWishlist_win.Sorting;
using static System.Windows.Forms.DialogResult;
using static System.Windows.Forms.MessageBoxButtons;
using System.Collections.Generic;

namespace CWishlist_win
{
    public partial class Form1 : Form
    {
        public readonly PluginManager plugin_manager = new PluginManager();
        public readonly ThreadManager thread_manager = new ThreadManager();
        public WL wl;
        public string current_file = "";
        public Item[] loaded_wl = EMPTY;
        public List<string> recents = new List<string>();
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
        public readonly string ver_str = "7.0.0a"; 
        public readonly uint ver_int = 0x700a;
        public readonly byte[] version = new byte[] { 7, 0, 0, 254 };
        public readonly object recents_mutex = new object();
        public readonly object backup_mutex = new object();
        //restore backup mutex
        public readonly object rbackup_mutex = new object();
        //screen list mutex
        public readonly object slist_mutex = new object();
        //backend list[s] mutex
        public readonly object blist_mutex = new object();

        int start(function f)
        {
            return thread_manager.start(f);
        }

        void join(int id)
        {
            thread_manager.join(id);
        }

        public Form1()
        {
            InitializeComponent();

            if (args.Length > 0)
                load_wl(args[0]);
            else
                lock (blist_mutex) { wl = NEW; }
            
            int i = start(() =>
            {
                if (!Directory.Exists(appdir))
                {
                    Directory.CreateDirectory(appdir);
                }
            });

            int j = start(() =>
            {
                if (!Directory.Exists(lang_dir))
                    Directory.CreateDirectory(lang_dir);
            });

            start(() =>
            {
                if (!Directory.Exists(plugin_dir))
                    Directory.CreateDirectory(plugin_dir);
            });

            int k = start(() =>
            {
                join(j);
                if (!File.Exists(lang_de) || !arrequ(utf8(de_lang_xml), File.ReadAllBytes(lang_de)))
                    File.WriteAllText(lang_de, de_lang_xml);
            });

            int l = start(() =>
            {
                join(j);
                if (!File.Exists(lang_en) || !arrequ(utf8(en_lang_xml), File.ReadAllBytes(lang_en)))
                    File.WriteAllText(lang_en, en_lang_xml);
            });

            start(() =>
            {
                join(i);
                lock (recents_mutex)
                    if (File.Exists(recents_file))
                        try
                        {
                            recents = load_recents(recents_file);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Corrupted recents:\n\n" + e);
                            try
                            {
                                write_recents(recents_file, recents);
                            }
                            catch (Exception e1)
                            {
                                MessageBox.Show("Unable to write new recents:\n\n" + e1);
                                try
                                {
                                    File.Delete(recents_file);
                                }
                                catch (Exception e2)
                                {
                                    MessageBox.Show("Can't even delete the old recents:\n\n" + e2);
                                    MessageBox.Show("I guess you really blew up your PC...");
                                }
                            }
                        }
                    else
                        write_recents(recents_file, recents);
            });

            start(() =>
            {
                join(k);
                join(l);

                foreach (string f in Directory.GetFiles(lang_dir))
                    load_lang_xml(f);

                if (File.Exists(lang_file))
                    select_lang(ascii(File.ReadAllBytes(lang_file)));
                else if (File.Exists(legacy_lang_file))
                {
                    byte[] c = File.ReadAllBytes(legacy_lang_file);
                    if (c.Length == 1)
                        select_lang(c[0] == 0 ? "en" : "de");
                    else
                        select_lang(ascii(c));
                }

                if (!File.Exists(restore_backup))
                    File.WriteAllBytes(restore_backup, new byte[] { 0 });
                else if (File.ReadAllBytes(restore_backup)[0] != 0 &&
                GetProcessesByName("CWishlist_win").Length < 2 &&
                MessageBox.Show(get_translated("prompt.restore_backup"),
                get_translated("caption.restore_backup"), YesNo) == Yes)
                    lock (backup_mutex) { wl = backup_load(backup_file); }
            });

            start(load_width);
            start(load_height);
            start(load_color);

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

        void load_width()
        {
            if (File.Exists(width_file))
                Width = int32(File.ReadAllBytes(width_file));
            else if (File.Exists(legacy_width_file))
            {
                Width = int32(File.ReadAllBytes(legacy_width_file));
                File.Delete(legacy_width_file);
            }
        }

        void load_height()
        {
            if (File.Exists(height_file))
                Height = int32(File.ReadAllBytes(height_file));
            else if (File.Exists(legacy_height_file))
            {
                Height = int32(File.ReadAllBytes(legacy_height_file));
                File.Delete(legacy_height_file);
            }
        }

        void load_color()
        {
            if (File.Exists(color_file))
            {
                byte[] b = File.ReadAllBytes(color_file);
                set_color(b[0], b[1], b[2]);
            }
            else if (File.Exists(legacy_color_file))
            {
                set_color(int32(File.ReadAllBytes(legacy_color_file)));
                File.Delete(legacy_color_file);
            }
        }

        public void update_ui()
        {
            //no GC for [up to] 15MiB of small object heap and 127MiB of big object heap
            try
            {
                GC.TryStartNoGCRegion(15 * 1024 * 1024 + 1024 * 1024 * 127,
                    127 * 1024 * 1024, true);
            }
            catch (Exception e)
            {
                MessageBox.Show("Can't start no GC area:\n" + e);
            }
            
            recentToolStripMenuItem.DropDownItems.Clear();
            if (recents.Count > 0)
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
                recentToolStripMenuItem.DropDownItems.Add(new ToolStripMenuItem(NA));
            asynctinyflush();
            start(() => {  });
            start(() => {  });
            int index = listBox1.SelectedIndex;
            thread_manager.finishall();
            listBox1.Items.Clear();
            foreach (Item i in wl.items)
                listBox1.Items.Add(i.ToString());
            textBox1.Visible = textBox2.Visible = label1.Visible = label2.Visible = button4.Visible = button5.Visible = button6.Visible = false;
            listBox1.SelectedIndex = index;
            Invalidate();
            Update();
            GC.EndNoGCRegion();
            GC.Collect(2, GCCollectionMode.Forced, false, true);
        }

        void update_ui_save_backup()
        {
            try
            {
                lock (backup_mutex)
                {
                    backup_save(wl, backup_file);
                }
            }
            catch { }
        }

        void update_ui_write_restore_backup()
        {
            try
            {
                lock (rbackup_mutex)
                {
                    writesbf(restore_backup, 1);
                }
            }
            catch { }
        }

        public void asynctinyflush()
        {
            foreach (Item i in wl)
                if (!i.url.StartsWith(tinyurl) && valid_url(i.url))
                    start(() => asynctinyflush_worker(i));
        }

        public void asynctinyflush_f()
        {
            foreach (Item i in wl)
                if (!i.url.StartsWith(tinyurl) && valid_url(i.url))
                    start(() => asynctinyflush_worker(i));
            thread_manager.finishall();
        }

        void asynctinyflush_worker(Item i)
        {
            i.url = tinyurl_create(i.url);
        }

        public void try_update_ui()
        {
            try
            {
                update_ui();
            }
            catch { }
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

        void add_item(object sender, EventArgs e)
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

        void btn4_click(object _, EventArgs e)
        {
            if (ContainsText())
                start(() =>
                {
                    //sometimes pasting fails randomly so we're repeating until it pastes
                    for (int i = 0; i < int.MaxValue; i++)
                        try
                        {
                            textBox1.Text = GetText();
                            break;
                        }
                        catch { }
                });
        }

        void btn5_click(object _, EventArgs e)
        {
            if (ContainsText())
                start(() =>
                {
                    //sometimes pasting fails randomly so we're repeating until it pastes
                    for (int i = 0; i < int.MaxValue; i++)
                        try
                        {
                            textBox2.Text = GetText();
                            break;
                        }
                        catch { }
                });
        }

        void btn6_click(object _, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            string url = wl.items[listBox1.SelectedIndex].url;
            Start(valid_url(url) ? url : http + url);
        }

        void remove_click(object _, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            Item[] old = wl.items;
            wl.items = new Item[old.Length - 1];
            arrcpy(old, wl.items, listBox1.SelectedIndex);
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
            button1.Location = new Point(w - 271, h2 - 49);
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
            if ((wl > 0 && current_file == "") || (current_file != "" && !arrequ(wl.items, loaded_wl)))
            {
                bool flag = MessageBox.Show(get_translated("prompt.close"), get_translated("caption.close"), YesNo) == No;
                e.Cancel = flag;
                if (flag)
                    return;
            }
            if (current_file != "")
                start(add_current_file_to_recent_items);
            start(write_recents_mutexed_close);
            start(disable_restore_backup_close);
            start(write_lang_file_close);
            start(write_wid_file_close);
            start(write_hei_file_close);
            start(write_color_file_close);
            Hide();
            thread_manager.shutdown();
        }

        void write_color_file_close()
        {
            Color c = BackColor;
            File.WriteAllBytes(color_file, new byte[] { c.R, c.G, c.B });
        }

        void write_hei_file_close()
        {
            File.WriteAllBytes(height_file, bytes(Height));
        }

        void write_wid_file_close()
        {
            File.WriteAllBytes(width_file, bytes(Width));
        }

        void write_lang_file_close()
        {
            File.WriteAllBytes(lang_file, ascii(selected.code));
        }

        void disable_restore_backup_close()
        {
            lock (rbackup_mutex)
                writesbf(restore_backup, 0);
        }

        void write_recents_mutexed_close()
        {
            lock (recents_mutex)
                write_recents(recents_file, recents);
        }

        public void add_current_file_to_recent_items()
        {
            string s = current_file;
            start(() =>
            {
                lock (recents_mutex)
                {
                    if (recents.Count > 0 && recents[0] == s)
                        return;
                    recents.Insert(0, s);
                }
            });
        }

        void new_click(object _, EventArgs e)
        {
            if ((wl > 0 && current_file == "") || (current_file != "" && !arrequ(wl.items, loaded_wl))
                && MessageBox.Show(get_translated("prompt.new"), get_translated("caption.new"), YesNo) == No)
                    return;
            lock (blist_mutex)
            {
                if (current_file != "")
                {
                    add_current_file_to_recent_items();
                    current_file = "";
                }
                wl = NEW;
                loaded_wl = EMPTY;
            }
            try_update_ui();
        }

        void open_click(object _, EventArgs e)
        {
            if (((current_file == "" && wl.Length > 0) || (current_file != "" && !arrequ(wl.items, loaded_wl))) &&
                MessageBox.Show(get_translated("prompt.open"), get_translated("caption.open"), YesNo) == No)
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
                    add_current_file_to_recent_items();
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
                current_file = sfd.FileName;
                add_current_file_to_recent_items();
                cwld_save(wl, current_file);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.S: save_click(null, null); break;
                case Keys.Control | Keys.Shift | Keys.S: save_as_click(null, null); break;
                case Keys.Control | Keys.O: open_click(null, null); break;
                case Keys.Control | Keys.N: new_click(null, null); break;
                case Keys.Up: if (listBox1.SelectedIndex != -1) listBox1.SelectedIndex--; break;
                case Keys.Down: if (listBox1.SelectedIndex < listBox1.Items.Count - 1) listBox1.SelectedIndex++; break;
                default: return base.ProcessCmdKey(ref msg, keyData);
            }
            update_ui();
            return true;
        }

        void move_up_click(object _, EventArgs e)
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

        void move_down_click(object _, EventArgs e)
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

        void btn8_click(object _, EventArgs e)
        {
            foreach(Item i in wl)
                Start(i.url.StartsWith(http) || i.url.StartsWith(ftp) ? i.url
                    : http + i.url);
        }

        void chnglg_click(object _, EventArgs e)
        {
            string tmp = Path.GetTempFileName();
            File.WriteAllLines(tmp, get_translated("misc.changelog"));
            Start("notepad", $"\"{tmp}\"");
        }

        /// <summary>
        /// Opens the GitHub repo in the default browser.
        /// </summary>
        void version_click(object _, EventArgs e)
        {
            Start("https://github.com/chrissxYT/CWishlist_win");
        }

        void sort_click(object _, EventArgs e)
        {
            asynctinyflush_f();
            lock (blist_mutex)
                quicksort(0, wl.Length - 1, ref wl.items);
            update_ui();
        }

        void lang_click(object _, EventArgs e)
        {
            new LanguageSelectionDialog(get_translated("title.switch_lang")).ShowDialog();
        }

        void search_change(object _, EventArgs e)
        {
            string s = textBox3.Text.ToLower();
            int[] i = wl.GetIndices((it) => it.name.ToLower().Contains(s));
            if (i.Length > 0)
            {
                listBox1.SelectedIndex = i[0];
                foreach (int j in i)
                    listBox1.Items[j] = "* " + listBox1.Items[j];
            }
        }

        /// <summary>
        /// Invoked on click on the search box, selects all the text in the search box.
        /// </summary>
        void search_click(object _, EventArgs e)
        {
            textBox3.Focus();
            textBox3.SelectionStart = 0;
            textBox3.SelectionLength = textBox3.TextLength;
        }

        /// <summary>
        /// Sets all the colors to the given one.
        /// </summary>
        public void set_color(string s)
        {
            byte[] b = hex(s);
            set_color(b[0], b[1], b[2]);
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
			BackColor = listBox1.BackColor = textBox1.BackColor =
            textBox2.BackColor = textBox3.BackColor = button1.BackColor =
            button2.BackColor = button3.BackColor = button4.BackColor =
            button5.BackColor = button6.BackColor = button7.BackColor =
            button8.BackColor = button9.BackColor = menuStrip1.BackColor = c;
		}

        /// <summary>
        /// Opens an InputBox for the color.
        /// </summary>
        void style_click(object sender, EventArgs e)
        {
            try
            {
                set_color(Interaction.InputBox("Please enter a hex value:",
                    "background color hex", "FFFFFF"));
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
        void paint(object sender, PaintEventArgs e)
        {
            plugin_manager.call_paint_listeners(e, this);
        }

        /// <summary>
        /// Loads the CWL from the given file to loaded_wl, wl and current_file.
        /// </summary>
        /// <param name="file">The file to load the CWL from.</param>
        public void load_wl(string file)
        {
            lock (blist_mutex)
            {
                wl = load(file);
                current_file = file;
                loaded_wl = new Item[wl];
                farrcpy(wl, loaded_wl);
            }
            try_update_ui();
        }

        /// <summary>
        /// Shows the DebugTools as a dialog.
        /// </summary>
        void debugToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new DebugTools(this).ShowDialog();
        }

        void _3rd_party_software(object sender, EventArgs e)
        {
            MessageBox.Show(
                "This software uses the LZMA SDK by Igor Pavlov.\n" +
                "While it is Public Domain I still wanted to give credit.\n" +
                "So go to 7-zip.org!",
                "A little bit of credits...");
        }
    }
}
