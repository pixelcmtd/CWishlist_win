using System.Windows.Forms;

namespace CWishlist_win
{
    public interface IPlugin
    {
        string name { get; }
        string ver_str { get; }
        byte[] version { get; }
        uint ver_int { get; }
        string update_url { get; }
    }

    public interface IFormConstructListener
    {
        void form_contruct();
    }

    public interface IPaintListener
    {
        void paint(PaintEventArgs e);
    }
}
