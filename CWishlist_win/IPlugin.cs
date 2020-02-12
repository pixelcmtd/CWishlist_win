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

        /// <summary>
        /// Basically a replacement for a constructor.
        /// </summary>
        void construct(PluginManager plugin_manager);

        /// <summary>
        /// Checks wheather this plugin-version is compatible with this version of CWishlist_win
        /// </summary>
        /// <returns>true if this plugin-version is compatible with this version of CWishlist_win, false if not</returns>
        bool is_compatible(string vs, byte[] vb, uint vi);
    }
}
