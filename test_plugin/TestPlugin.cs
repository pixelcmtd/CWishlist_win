using CWishlist_win;
using System.Drawing;
using System.Windows.Forms;

namespace test_plugin
{
    public class TestPlugin : IPlugin, IFormConstructListener, IPaintListener
    {
        public string name = "Test plugin";

        public string ver_str = "1.0.0";

        public byte[] version = new byte[] { 1, 0, 0 };

        public uint ver_int = 1;

        public string update_url = "NA";

        string IPlugin.name => name;

        string IPlugin.ver_str => ver_str;

        byte[] IPlugin.version => version;

        uint IPlugin.ver_int => ver_int;

        string IPlugin.update_url => update_url;

        public void construct(PluginManager plugin_manager)
        {
            plugin_manager.register_form_construct_listener(this);
            plugin_manager.register_paint_listener(this);
        }

        public void form_contruct(Form1 form)
        {
            MessageBox.Show($"TestPlugin, stack size: {form.stack_size}");
        }

        public void paint(PaintEventArgs e, Form1 form)
        {
            e.Graphics.DrawLine(Pens.Brown, 10, 10, 200, 500);
        }
    }
}
