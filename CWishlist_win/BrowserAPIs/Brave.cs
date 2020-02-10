namespace CWishlist_win
{
    static class Brave
    {
        public static string bookmark_path_from_appdata_local(string appdata_local)
        {
            return appdata_local + @"\BraveSoftware\Brave-Browser\User Data\Default\Bookmarks";
        }

        public static bookmarks parse(string json_file) => Chromium.parse(json_file);
    }
}
