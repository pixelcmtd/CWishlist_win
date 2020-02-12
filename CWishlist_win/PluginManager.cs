using static CWishlist_win.Properties.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace CWishlist_win
{
    public delegate void form_construct_listener(Form1 form);
    public delegate void paint_listener(PaintEventArgs e, Form1 form);

    public class PluginManager
    {
        List<form_construct_listener> form_construct_listeners = new List<form_construct_listener>();
        List<paint_listener> paint_listeners = new List<paint_listener>();
        Dictionary<IPlugin, string> plugins = new Dictionary<IPlugin, string>();
        Form1 form;

        public PluginManager(Form1 form)
        {
            this.form = form;
        }

        public void register_plugin(IPlugin plugin, string file)
        {
            plugins.Add(plugin, file);
            plugin.construct(this);
        }

        public void register_form_construct_listener(form_construct_listener listener) => form_construct_listeners.Add(listener);
        public void register_paint_listener(paint_listener listener) => paint_listeners.Add(listener);

        public void call_form_construct_listeners()
        {
            foreach (form_construct_listener l in form_construct_listeners)
                l(form);
        }

        public void call_paint_listeners(PaintEventArgs e)
        {
            foreach (paint_listener l in paint_listeners)
                l(e, form);
        }

        public void load_plugins(string file)
        {
            Assembly asm = Assembly.LoadFile(file);
            foreach (Type t in asm.ExportedTypes)
                if (typeof(IPlugin).IsAssignableFrom(t))
                    register_plugin((IPlugin)Activator.CreateInstance(t), file);
        }
		
		public void update_check(IPlugin plugin)
		{
			try
			{
                XmlReader xml = XmlReader.Create(new WebClient().OpenRead(plugin.update_url));
                uint s_ver = 0;
                string url = "";
				while(xml.Read())
					if(xml.Name == "update_info")
                    {
                        s_ver = uint.Parse(xml.GetAttribute("version"));
                        url = xml.GetAttribute("url");
                    }
                if(s_ver > plugin.ver_int)
                {
                    string tmp = Path.ChangeExtension(Path.GetTempFileName(), "exe");
                    File.WriteAllBytes(tmp, file_replace);
                    Process.Start(tmp,
                        $"{url} \"{plugins[plugin]}\" \"{Process.GetCurrentProcess().MainModule.FileName}\"");
                    form.start(() => File.Delete(tmp));
                    form.Close();
                    Environment.Exit(0);
                }
			}
			catch { }
		}
    }
}
