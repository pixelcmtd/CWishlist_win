using Microsoft.VisualBasic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static CWishlist_win.CLinq;
using static binutils.bin;
using static binutils.c;
using static binutils.str;
using static binutils.io;
using static CWishlist_win.Languages;
using static CWishlist_win.Properties.Resources;
using static CWishlist_win.Program;
using static CWishlist_win.Consts;
using static System.Diagnostics.Process;
using static CWishlist_win.IO;
using static CWishlist_win.WL;
using static CWishlist_win.Item;
using static System.Windows.Forms.Clipboard;
using static CWishlist_win.Sorting;
using static System.Windows.Forms.DialogResult;
using static System.Windows.Forms.MessageBoxButtons;
using System.Collections.Generic;
using System.Text;

namespace CWishlist_win
{
    public partial class Form1 : Form
    {
        public readonly PluginManager plugin_manager;
        public readonly ThreadManager thread_manager = new ThreadManager();
        public WL wl;
        public string current_file = "";
        public string cl_exe  = "$URL";
        public string cl_args = "";
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
        public readonly string cl_exe_file = appdata + "\\CWishlist\\CLE";
        public readonly string cl_args_file = appdata + "\\CWishlist\\CLA";
        public readonly object recents_mutex = new object();
        public readonly object backup_mutex = new object();
        public readonly object rbackup_mutex = new object(); //restore backup mutex
        public readonly object slist_mutex = new object(); //screen list mutex
        public readonly object blist_mutex = new object(); //backend list[s] mutex

        public int start(function f) => thread_manager.start(f);
        public void join(int id) => thread_manager.join(id);

        public Form1()
        {
            dbg("[Form1()]Constructing Form1...");

            plugin_manager = new PluginManager(this);
            InitializeComponent();

            dbg("[Form1()]Constructed Form1.");
        }

        public void init()
        {
            dbg("[Form1.init()]Form1 initializing...");

            if (args.Length > 0) load_wl(args[0]);
            else lock (blist_mutex) wl = NEW;

            int appdir_create_pid = start(() =>
            { if (!Directory.Exists(appdir)) Directory.CreateDirectory(appdir); });

            int langdir_create_pid = start(() =>
            { if (!Directory.Exists(lang_dir)) Directory.CreateDirectory(lang_dir); });

            start(() =>
            { if (!Directory.Exists(plugin_dir)) Directory.CreateDirectory(plugin_dir); });

            int save_langs_if_not_exist_pid = start(() =>
            {
                join(langdir_create_pid);
                if (!File.Exists(lang_de))
                    File.WriteAllText(lang_de, de_lang_xml);
                if (!File.Exists(lang_en))
                    File.WriteAllText(lang_en, en_lang_xml);
            });

            start(() =>
            {
                join(appdir_create_pid);
                lock (recents_mutex)
                    if (File.Exists(recents_file))
                        try
                        {
                            recents = load_recents(recents_file);
                            dbg("[Form1-InitThread]Read {0} recent files.", recents.Count);
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
                                try { File.Delete(recents_file); }
                                catch (Exception e2)
                                {
                                    MessageBox.Show("Can't even delete the old recents:\n\n" + e2,
                                        "I guess you really blew up your PC...");
                                }
                            }
                        }
                    else write_recents(recents_file, recents);
            });

            start(() =>
            {
                join(save_langs_if_not_exist_pid);

                load_langs(lang_dir);

                bool reload_langs = false;

                if (get_lang("de").version < ver_int)
                {
                    File.WriteAllText(lang_de, de_lang_xml);
                    reload_langs = true;
                }

                if (get_lang("en").version < ver_int)
                {
                    File.WriteAllText(lang_en, en_lang_xml);
                    reload_langs = true;
                }

                if (reload_langs)
                {
                    clear_langs();
                    load_langs(lang_dir);
                }

                if (File.Exists(lang_file)) select_lang(File.ReadAllText(lang_file, Encoding.ASCII));
                else if (File.Exists(legacy_lang_file))
                {
                    byte[] c = File.ReadAllBytes(legacy_lang_file);
                    if (c.Length == 1) select_lang(c[0] == 0 ? "en" : "de");
                    else               select_lang(ascii(c));
                }

                if (!File.Exists(restore_backup))
                    File.WriteAllBytes(restore_backup, new byte[] { 0 });
                else if (File.ReadAllBytes(restore_backup)[0] != 0 &&
                    GetProcessesByName("CWishlist_win").Length < 2 &&
                    MessageBox.Show(get_translated("prompt.restore_backup"),
                                    get_translated("caption.restore_backup"),
                                    YesNo) == Yes)
                    lock (backup_mutex) wl = backup_load(backup_file);
            });

            load_width();
            load_height();
            start(load_cl);
            load_color();

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

                plugin_manager.call_form_construct_listeners();
#endif
            thread_manager.finishall();
            update_ui();
            label3.Visible = false;

            dbg("[Form1.init()]Form1 initialized.");
        }

        void load_width()
        {
            if (File.Exists(width_file)) Width = int32(File.ReadAllBytes(width_file));
            else if (File.Exists(legacy_width_file))
            {
                Width = int32(File.ReadAllBytes(legacy_width_file));
                File.Delete(legacy_width_file);
            }
        }

        void load_height()
        {
            if (File.Exists(height_file)) Height = int32(File.ReadAllBytes(height_file));
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
                Color c = Color.FromArgb(b[0], b[1], b[2]);
                dbg("[Form1().load_color()]Read color {0} from new color file.", c);
                set_color(c);
            }
            else if (File.Exists(legacy_color_file))
            {
                Color c = Color.FromArgb(int32(File.ReadAllBytes(legacy_color_file)));
                dbg("[Form1().load_color()]Read color {0} from old color file.", c);
                set_color(c);
                File.Delete(legacy_color_file);
            }
        }

        void load_cl()
        {
            if(File.Exists(cl_exe_file))
                cl_exe = File.ReadAllText(cl_exe_file);
            if (File.Exists(cl_args_file))
                cl_args = File.ReadAllText(cl_args_file);
            dbg("[Form1.load_cl()]Read cl_exe \"{0}\" and cl_args \"{1}\".", cl_exe, cl_args);
        }
        
        public void update_ui()
        {
            try
            {
                //no GC for [up to] 15MiB of small object heap and 127MiB of large object heap
                GC.TryStartNoGCRegion((15 + 127) * 1024 * 1024, 127 * 1024 * 1024, true);
            }
            catch (Exception e) { dbg("[Form1.update_ui()]Can't start no GC area: {0}", b64(e)); }
            recentToolStripMenuItem.DropDownItems.Clear();
            if (recents.Count > 0)
                foreach (string r in recents)
                {
                    ToolStripMenuItem itm = new ToolStripMenuItem(r);
                    itm.Click += new EventHandler((sender, e) =>
                    {
                        add_current_file_to_recent_items();
                        load_wl(r);
                        update_ui();
                    });
                    recentToolStripMenuItem.DropDownItems.Add(itm);
                }
            else recentToolStripMenuItem.DropDownItems.Add(new ToolStripMenuItem(NA));
            asynctinyflush();
            int index = listBox1.SelectedIndex;
            thread_manager.finishall();
            start(update_ui_save_backup);
            start(update_ui_write_restore_backup);
            listBox1.Items.Clear();
            foreach (Item i in wl.items) listBox1.Items.Add(i.ToString());
            textBox1.Visible = textBox2.Visible = label1.Visible = label2.Visible
                = button4.Visible = button5.Visible = button6.Visible = false;
            listBox1.SelectedIndex = index;
            thread_manager.finishall();
            Invalidate();
            Update();
            GC.EndNoGCRegion();
            GC.Collect(2, GCCollectionMode.Forced, false, true);
        }

        void update_ui_save_backup()
        { try { lock (backup_mutex) backup_save(wl, backup_file); } catch { } }

        void update_ui_write_restore_backup()
        { try { lock (rbackup_mutex) { File.WriteAllBytes(restore_backup, new byte[] { 1 }); } } catch { } }

        public void asynctinyflush()
        {
            foreach (Item i in wl)
                if (!i.url.StartsWith(tinyurl) && valid_url(i.url))
                    start(() => asynctinyflush_worker(i));
        }

        public void asynctinyflush_f()
        {
            asynctinyflush();
            thread_manager.finishall();
        }

        void asynctinyflush_worker(Item i) => i.url = tinyurl_create(i.url);
        public void try_update_ui() { try { update_ui(); } catch { } }

        void lstbx_index_change(object sender, EventArgs e)
        {
			bool f = listBox1.SelectedIndex != -1;
            textBox1.Visible = textBox2.Visible = label1.Visible = label2.Visible
                = button4.Visible = button5.Visible = button6.Visible = f;
			if (f)
			{
				textBox1.Text = wl.items[listBox1.SelectedIndex].name;
                textBox2.Text = wl.items[listBox1.SelectedIndex].url;
			}
            textBox1.Focus();
        }

        void add_item(object sender, EventArgs e)
        {
            Item[] old = wl.items;
            wl.items = new Item[old.Length + 1];
            for (int i = 0; i < old.Length; i++) wl.items[i] = old[i];
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
                        catch (Exception e1)
                        {
                            dbg(e1.ToString());
                        }
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
                        catch (Exception e1)
                        {
                            dbg(e1.ToString());
                        }
                });
        }

        void btn6_click(object _, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            string url  = wl.items[listBox1.SelectedIndex].url;
            string name = wl.items[listBox1.SelectedIndex].name;
            Start(cl_exe.Replace("$URL", url).Replace("$NAME", name),
                 cl_args.Replace("$URL", url).Replace("$NAME", name));
        }

        void remove_click(object _, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            Item[] old = wl.items;
            wl.items = new Item[old.Length - 1];
            arrcpy(old, wl.items, listBox1.SelectedIndex);
            for (int i = listBox1.SelectedIndex + 1; i < old.Length; i++) wl.items[i - 1] = old[i];
            try { update_ui(); } catch { listBox1.SelectedIndex = wl.Length - 1; }
        }

        void size_change(object _, EventArgs e)
        {
            int w = Width;
            int h = Height;
            int h2 = h / 2;
            button1.Location = new Point(w - 271, h2 - 49);
            button2.Location = new Point(w - 271, h2 - 10);
            button3.Location = new Point(w - 232, h2 - 49);
            button7.Location = new Point(w - 232, h2 - 10);
            listBox1.Size = new Size(w - 289, h - 93);
            button4.Location = new Point(w - 72, 26);
            button5.Location = new Point(w - 72, 46);
            button6.Location = new Point(w - 267, 67);
            button8.Location = new Point(w - 92, h - 71);
            button9.Location = new Point(w - 271, h - 71);
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
                DialogResult dr = MessageBox.Show(get_translated("prompt.close"),
                                                  get_translated("caption.close"),
                                                  YesNoCancel);
                if (dr == Yes   ) save_click(_, e);
           else if (dr == Cancel) return;
            }
            Hide();
            if (current_file != "")
                lock (recents_mutex)
                    if (recents.Count < 1 || recents[0] != current_file)
                    {
                        recents.RemoveAll((t) => t == current_file);
                        recents.Insert(0, current_file);
                    }
            start(write_recents_mutexed_close);
            start(disable_restore_backup_close);
            start(write_lang_file_close);
            start(write_wid_file_close);
            start(write_hei_file_close);
            start(write_color_file_close);
            start(write_cl_exe_file_close);
            start(write_cl_args_file_close);
            thread_manager.shutdown();
        }
        
        void write_color_file_close()
        {
            Color c = BackColor;
            File.WriteAllBytes(color_file, new byte[] { c.R, c.G, c.B });
        }

        void write_hei_file_close() => File.WriteAllBytes(height_file, bytes(Height));
        void write_wid_file_close() => File.WriteAllBytes(width_file, bytes(Width));
        void write_lang_file_close() => File.WriteAllBytes(lang_file, ascii(selected.code));
        void write_cl_exe_file_close() => File.WriteAllText(cl_exe_file, cl_exe);
        void write_cl_args_file_close() => File.WriteAllText(cl_args_file, cl_args);
        void disable_restore_backup_close() { lock (rbackup_mutex) File.WriteAllBytes(restore_backup, new byte[] { 0 }); }
        void write_recents_mutexed_close() { lock (recents_mutex) write_recents(recents_file, recents); }

        public void add_current_file_to_recent_items()
        {
            string s = current_file;
            start(() =>
            {
                lock (recents_mutex)
                {
                    if (recents.Count > 0 && recents[0] == s) return;
                    recents.RemoveAll((t) => t == s);
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
            DialogResult dr = No;
            //((file not saved && wl not empty) || (file saved && wl changed))
            //&& (ask user) == cancel
            if (((current_file == "" && wl.Length > 0) || (current_file != "" &&
                !arrequ(wl.items, loaded_wl))) && (dr = MessageBox.Show(get_translated("prompt.open"),
                                                                  get_translated("caption.open"),
                                                                  YesNoCancel)) == Cancel)
                return;
            else if (dr == Yes)
                save_click(_, e);
            OpenFileDialog ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "CWishlists|*.cwld;*.cwlu",
                Title = "Load CWishlist",
                Multiselect = false
            };
            DialogResult res = ofd.ShowDialog();
            if (res == Yes || res == DialogResult.OK)
            {
                if (current_file != "") add_current_file_to_recent_items();
                load_wl(ofd.FileName);
            }
        }

        void save_click(object _, EventArgs e)
        {
            if (current_file == "") save_as_click(_, e);
            else
            {
                int lm1 = current_file.Length - 1;
                if (current_file[lm1] != 'd')
                {
                    char[] c = current_file.ToCharArray();
                    c[lm1] = 'd';
                    current_file = new string(c);
                }
                update_ui();
                cwld_save(wl, current_file);
            }
        }

        void save_as_click(object _, EventArgs e)
        {
            update_ui();
            SaveFileDialog sfd = new SaveFileDialog()
            {
                AddExtension = true,
                ValidateNames = true,
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
                case Keys.Control | Keys.F: search_click(null, null); break;
                case Keys.Up: if (listBox1.SelectedIndex != -1) listBox1.SelectedIndex--; break;
                case Keys.Down: if (listBox1.SelectedIndex < listBox1.Items.Count - 1) listBox1.SelectedIndex++; break;
            }
            if (!text_mode())
            {
                switch (keyData)
                {
                    case Keys.J: if (listBox1.SelectedIndex < listBox1.Items.Count - 1) listBox1.SelectedIndex++; break;
                    case Keys.K: if (listBox1.SelectedIndex > -1) listBox1.SelectedIndex--; break;
                    case Keys.O: btn6_click(null, null); break;
                    case Keys.A: add_item(null, null); break;
                    case Keys.D: remove_click(null, null); break;
                    case Keys.S: sort_click(null, null); break;
                    case Keys.I: textBox1.Focus(); break;
                }
            }
            if (text_mode() && keyData == Keys.Escape)
            {
                listBox1.Focus();
            }
            update_ui();
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void move_up_click(object _, EventArgs e)
        {
            if (listBox1.SelectedIndex < 1) return;
            Item[] old = wl.items;
            wl.items = new Item[old.Length];
            int index = listBox1.SelectedIndex;
            for (int i = 0; i < index - 1; i++) wl.items[i] = old[i];
            wl.items[index - 1] = old[index];
            wl.items[index] = old[index - 1];
            for (int i = index + 1; i < old.Length; i++) wl.items[i] = old[i];
            update_ui();
            listBox1.SelectedIndex = index - 1;
        }

        void move_down_click(object _, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1 || listBox1.SelectedIndex == listBox1.Items.Count - 1) return;
            Item[] old = wl.items;
            Item[] nw = new Item[old.Length];
            int index = listBox1.SelectedIndex;
            for (int i = 0; i < index; i++) nw[i] = old[i];
            nw[index] = old[index + 1];
            nw[index + 1] = old[index];
            for (int i = index + 2; i < old.Length; i++) nw[i] = old[i];
            wl.items = nw;
            update_ui();
            listBox1.SelectedIndex = index + 1;
        }

        void btn8_click(object _, EventArgs e)
        {
            foreach(Item i in wl)
                Start(i.url.StartsWith(https) || i.url.StartsWith(http) ? i.url : https + i.url);
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
            string url = "https://github.com/chrissxYT/CWishlist_win";
            Start(cl_exe.Replace("$URL", url).Replace("$NAME", ""),
                 cl_args.Replace("$URL", url).Replace("$NAME", ""));
        }

        void sort_click(object _, EventArgs e)
        {
            asynctinyflush_f();
            lock (blist_mutex) quicksort(0, wl.Length - 1, ref wl.items);
            update_ui();
        }

        void lang_click(object _, EventArgs e)
        {
            new LanguageSelectionDialog(get_translated("title.switch_lang")).ShowDialog();
        }

        bool containsany(string s, string[] t)
        {
            foreach (string u in t) if (s.Contains(u)) return true;
            return false;
        }

        void search_change(object _, EventArgs e)
        {
            string[] s = textBox3.Text.ToLower().Split(' ', '_', '-');
            lock(blist_mutex)
            {
                int[] i = wl.GetIndices((it) => containsany(it.name.ToLower(), s));
                if (i.Length > 0)
                {
                    listBox1.SelectedIndex = i[0];
                    foreach (int j in i) listBox1.Items[j] = "* " + listBox1.Items[j];
                }
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
        public void set_color(string s) => set_color(hex(s));

        /// <summary>
        /// Sets all the colors to the given one.
        /// </summary>
        public void set_color(byte[] rgb) => set_color(rgb[0], rgb[1], rgb[2]);

        /// <summary>
        /// Sets all the colors to the given one.
        /// </summary>
        public void set_color(byte r, byte g, byte b) => set_color(Color.FromArgb(r, g, b));

        /// <summary>
        /// Sets all the colors to the given one.
        /// </summary>
        public void set_color(int argb) => set_color(Color.FromArgb(argb));
		
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
        void plugindir_click(object sender, EventArgs e) => Start("explorer", plugin_dir);

        void paint(object sender, PaintEventArgs e) => plugin_manager.call_paint_listeners(e);

        /// <summary>
        /// Loads the WL from the given file to loaded_wl, wl and current_file.
        /// </summary>
        /// <param name="file">The file to load the CWL from.</param>
        public void load_wl(string file)
        {
            lock (blist_mutex)
            {
                wl = load(file);
                current_file = file;
                loaded_wl = new Item[wl];
                farrcpyitm(wl, loaded_wl);
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

        void taskmanager(object sender, EventArgs e) => new TaskManager(this).Show();

        void listBox1_DoubleClick(object _, EventArgs e) => btn6_click(_, e);

        public bool text_mode() =>  textBox1.Focused || textBox2.Focused || textBox3.Focused;

        void OpenCommandLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new CLSettings(this).Show();
        }
    }
}
