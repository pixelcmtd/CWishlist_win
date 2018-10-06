namespace CWishlist_win
{
    public static class Consts
    {
        public static byte[] cwld_header { get; } = new byte[8] { 67, 87, 76, 68, 13, 10, 26, 10 }; //C W L D CR LF EOF LF
        public static byte[] cwls4_header { get; } = new byte[8] { 67, 87, 76, 83, 13, 10, 26, 10 }; //C W L S CR LF EOF LF
        public static byte[] cwls5_header { get; } = new byte[4] { 67, 87, 76, 83 };
        public static byte[] cwll_header { get; } = new byte[4] { 67, 87, 76, 76 }; //CWLL

        public static string tinyurl_api { get; } = "http://tinyurl.com/api-create.php?url=";
        public static string tinyurl { get; } = "http://tinyurl.com/";
    }
}
