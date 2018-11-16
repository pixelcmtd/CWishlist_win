using CWishlist_win.Properties;
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

        public void register_plugin(IPlugin plugin, string file)
        {
            plugins.Add(plugin, file);
            plugin.construct(this);
        }

        public void register_form_construct_listener(form_construct_listener listener) => form_construct_listeners.Add(listener);

        public void register_paint_listener(paint_listener listener) => paint_listeners.Add(listener);

        public void call_form_construct_listeners(Form1 form)
        {
            foreach (form_construct_listener l in form_construct_listeners)
                l(form);
        }

        public void call_paint_listeners(PaintEventArgs e, Form1 form)
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
                    File.WriteAllBytes(tmp, Environment.GetEnvironmentVariable(
                        "PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine) == "AMD64" ?
                        Resources.file_replace_64 : Resources.file_replace_32);
                    Process.Start(tmp,
                        $"{dll_dl_url} \"{plugins[plugin]}\" \"{Process.GetCurrentProcess().MainModule.FileName}\"");
                    Program.form.Close();
                    Environment.Exit(0);
                }
			}
			catch { }
		}
    }
}
