using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace CWishlist_win
{
    public class PluginManager : IConfigurationSectionHandler
    {
        List<IPlugin> plugins = new List<IPlugin>();
        List<IFormConstructListener> form_construct_listeners = new List<IFormConstructListener>();
        List<IPaintListener> paint_listeners = new List<IPaintListener>();

        public void register_plugin(IPlugin plugin) => plugins.Add(plugin);

        public void register_form_construct_listener(IFormConstructListener listener) => form_construct_listeners.Add(listener);

        public void register_paint_listener(IPaintListener listener) => paint_listeners.Add(listener);

        public void call_form_construct_listeners()
        {
            foreach (IFormConstructListener l in form_construct_listeners)
                l.form_contruct();
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
		
		public void update_check(IPlugin plugin)
		{
			try
			{
				byte[] dl = new WebClient().DownloadData(plugin.update_url);
				uint c_ver = plugin.ver_int;
				XmlReader xml = XmlReader.Create(new MemoryStream(dl));
				while(xml.Read())
					if(xml.Name == "")
						;
			}
			catch { }
		}
    }
}
