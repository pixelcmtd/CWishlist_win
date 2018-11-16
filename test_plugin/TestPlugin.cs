using CWishlist_win;
using System.Drawing;
using System.Windows.Forms;

namespace test_plugin
{
    public class TestPlugin : IPlugin
    {
        string IPlugin.name => "Test plugin";

        string IPlugin.ver_str => "1.0.0";

        byte[] IPlugin.version => new byte[] { 1, 0, 0 };

        uint IPlugin.ver_int => 1;

        string IPlugin.update_url => "";

        public void construct(PluginManager plugin_manager)
        {
            plugin_manager.register_form_construct_listener(form_construct);
            plugin_manager.register_paint_listener(paint);
        }

        public void form_construct(Form1 form)
        {
            MessageBox.Show("TestPlugin.");
        }

        public bool is_compatible(string vs, byte[] vb, uint vi)
        {
            return true;
        }

        public void paint(PaintEventArgs e, Form1 form)
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(12, 23, 90), 20), 10, 10, 200, 500);
        }
    }
}
