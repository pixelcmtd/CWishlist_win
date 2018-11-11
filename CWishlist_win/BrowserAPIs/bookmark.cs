namespace CWishlist_win
{
    struct bookmark
    {
        public string name;
        public string url;
        public int id;
        public long time;

        public bookmark(string name, string url, int id, long time)
        {
            this.name = name;
            this.url = url;
            this.id = id;
            this.time = time;
        }

        public string dbgstr(bool beautify = false, string tabs = null, string single_tab = null)
        {
            if (beautify)
            {
                return "{\n" + tabs + single_tab + "\"" + name + "\",\n" + tabs + single_tab
                    + "\"" + url + "\",\n" + tabs + single_tab + id + ",\n" + tabs +
                    single_tab + time + "\n" + tabs + "}";
            }
            else
            {
                return "{\"" + name + "\",\"" + url + "\"," + id + "," + time + "}";
            }
        }
    }
}
