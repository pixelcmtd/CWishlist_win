using System.Text;

namespace CWishlist_win
{
    struct bookmarks
    {
        public checksum checksum; //MD5 in Chrome (https://chromium.googlesource.com/chromium/src.git/+/master/components/bookmarks/browser/bookmark_codec.cc)
        public bookmark[] marks;

        public bookmarks(checksum checksum, bookmark[] marks)
        {
            this.checksum = checksum;
            this.marks = marks;
        }

        public string dbgstr(bool beautify = false, string tabs = null, string single_tab = null)
        {
            if (beautify)
            {
                string tab1 = tabs + single_tab;
                string tab2 = tab1 + single_tab;
                StringBuilder b = new StringBuilder("{\n" + tab1);
                b.Append(checksum.dbgstr(true, tab1, tab1));
                b.Append(",\n" + tab1 + "[\n" + tab2);
                b.Append(marks[0].dbgstr(true, tab2, single_tab));
                for (int i = 1; i < marks.Length; i++)
                {
                    b.Append(",\n" + tab2);
                    b.Append(marks[i].dbgstr(true, tab2, single_tab));
                }
                b.Append("\n" + tab1 + "]\n" + tabs + "}");
                return b.ToString();
            }
            else
            {
                StringBuilder b = new StringBuilder("{");
                b.Append(checksum.dbgstr());
                b.Append(",[");
                b.Append(marks[0].dbgstr());
                for (int i = 1; i < marks.Length; i++)
                {
                    b.Append(",");
                    b.Append(marks[i].dbgstr());
                }
                b.Append("]}");
                return b.ToString();
            }
        }
    }
}
