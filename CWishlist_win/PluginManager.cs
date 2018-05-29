﻿using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace CWishlist_win
{
    public class PluginManager : IConfigurationSectionHandler
    {
        static string file_replace = "https://github.com/chrissxYT/CWishlist_win/raw/master/file_replace/build/file_replace.exe";
        List<IPlugin> plugins = new List<IPlugin>();
        List<IFormConstructListener> form_construct_listeners = new List<IFormConstructListener>();
        List<IPaintListener> paint_listeners = new List<IPaintListener>();
        Dictionary<IPlugin, string> plugin_files = new Dictionary<IPlugin, string>();

        public void register_plugin(IPlugin plugin, string file)
        {
            plugins.Add(plugin);
            plugin_files.Add(plugin, file);
        }

        public void register_form_construct_listener(IFormConstructListener listener) => form_construct_listeners.Add(listener);

        public void register_paint_listener(IPaintListener listener) => paint_listeners.Add(listener);

        public void call_form_construct_listeners(Form1 form)
        {
            foreach (IFormConstructListener l in form_construct_listeners)
                l.form_contruct(form);
        }

        public void call_paint_listeners(PaintEventArgs e)
        {
            foreach (IPaintListener l in paint_listeners)
                l.paint(e);
        }

        public void load_plugin(string file)
        {

        }

        public object Create(object parent, object configContext, XmlNode section)
        {
            return null;
        }
		
		public void update_check(IPlugin plugin, string plugin_file)
		{
			try
			{
				byte[] dl = new WebClient().DownloadData(plugin.update_url);
				uint c_ver = plugin.ver_int;
                uint s_ver = 0;
                string dll_dl_url = "";
				XmlReader xml = XmlReader.Create(new MemoryStream(dl));
				while(xml.Read())
					if(xml.Name == "update_info")
                    {
                        s_ver = uint.Parse(xml.GetAttribute("version"));
                        dll_dl_url = xml.GetAttribute("url");
                    }
                if(s_ver > c_ver)
                {
                    string tmp = Path.ChangeExtension(Path.GetTempFileName(), "exe");
                    new WebClient().DownloadFile(file_replace, tmp);
                    Process.Start(tmp, string.Format("{0} \"{1}\" \"{2}\"", dll_dl_url, plugin_file, Process.GetCurrentProcess().MainModule.FileName));
                    Program.form.Close();
                }
			}
			catch { }
		}
    }
}
