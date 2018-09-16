namespace CWishlist_win
{
    public static class Consts
    {
        public static byte[] cwld_header { get; } = new byte[8] { 67, 87, 76, 68, 13, 10, 26, 10 }; //C W L D CR LF EOF LF
        public static byte[] cwls_header { get; } = new byte[8] { 67, 87, 76, 83, 13, 10, 26, 10 }; //C W L S CR LF EOF LF
        public static byte[] cwll_header { get; } = new byte[4] { 67, 87, 76, 76 }; //CWLL

        public static string tinyurl_api { get; } = "http://tinyurl.com/api-create.php?url=";

        public static byte cwll_utf8_base { get; } = 0b00010000;
        public static byte cwll_utf16_base { get; } = 0b11100000;
        public static byte cwll_tinyurl { get; } = 0b00001111;
        public static byte cwll_http { get; } = 0b00001000;
        public static byte cwll_https { get; } = 0b00000100;
        public static byte cwll_www { get; } = 0b00000010;
        public static byte cwll_utf8 { get; } = 0b00000001;

        public static string http { get; } = "http://";
        public static string https { get; } = "https://";
        public static string www { get; } = "www.";
    }
}
