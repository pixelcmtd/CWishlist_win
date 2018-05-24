using System.Windows.Forms;

namespace CWishlist_win
{
    public interface IPlugin
    {
        /// <summary>
        /// The name of the plugin (without the version)
        /// </summary>
        string name { get; }
        /// <summary>
        /// The version in a string representation
        /// </summary>
        string ver_str { get; }
        /// <summary>
        /// The version in the following syntax: version[0].version[1].version[2].version[3]
        /// </summary>
        byte[] version { get; }
        /// <summary>
        /// The numeric representation of the version that is used for updating
        /// </summary>
        uint ver_int { get; }
        /// <summary>
        /// The url location of the update-xml
        /// </summary>
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
