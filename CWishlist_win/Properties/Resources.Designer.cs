namespace CWishlist_win.Properties
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public class Resources
    {
        static global::System.Resources.ResourceManager resourceMan;
        static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {}
        
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                    resourceMan = new global::System.Resources.ResourceManager("CWishlist_win.Properties.Resources", typeof(Resources).Assembly);
                return resourceMan;
            }
        }
        
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        /// <summary>
        ///   german lang xml
        /// </summary>
        public static string de_lang_xml { get { return ResourceManager.GetString("de_lang_xml", resourceCulture); } }
        
        /// <summary>
        ///   english lang xml
        /// </summary>
        public static string en_lang_xml { get { return ResourceManager.GetString("en_lang_xml", resourceCulture); } }

        /// <summary>
        ///   the binary that replaces files around
        /// </summary>
        public static byte[] file_replace { get { return (byte[])ResourceManager.GetObject("file_replace", resourceCulture); } }
    }
}
