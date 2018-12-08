namespace CWishlist_win
{
    public static class Consts
    {
        public const string ver_str = "7.0.0b1"; //7.0.0 beta 1
        public const uint ver_int = 0x00700b01;
        public static readonly byte[] version = new byte[] { 7, 0, 0, 255, 1 };

        public static readonly byte[] cwld_header = new byte[8] { 67, 87, 76, 68, 13, 10, 26, 10 }; //C W L D CR LF EOF LF
        public static readonly byte[] cwls4_header = new byte[8] { 67, 87, 76, 83, 13, 10, 26, 10 }; //C W L S CR LF EOF LF
        public static readonly byte[] cwls_header = new byte[4] { 67, 87, 76, 83 }; //CWLS > 4 header
        public static readonly byte[] cwll_header = new byte[4] { 67, 87, 76, 76 }; //CWLL

        public const string tinyurl_api = "http://tinyurl.com/api-create.php?url=";
        public const string tinyurl = "http://tinyurl.com/";
        public const int tinyurl_length = 19;

        public const string NA = "N/A";

        public const string http = "http://";
        public const string https = "https://";
        public const string ftp = "ftp://";

        public const int D1 = 0;
        public const int D2 = 1;
        public const int L1 = 2;

        public const string nullstr = "";

        public const lang nulllang = new lang(null, null);

        //CWLLv1 strings

        //is a tinyurl
        public const byte L1_TU = 11;
        //is no tinyurl
        public const byte L1_NOTU = 8;
        //end string
        public const byte L1_ENDSTR = 11;
    }
}
