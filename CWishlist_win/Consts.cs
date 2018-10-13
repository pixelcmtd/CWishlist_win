﻿namespace CWishlist_win
{
    public static class Consts
    {
        public static readonly byte[] cwld_header = new byte[8] { 67, 87, 76, 68, 13, 10, 26, 10 }; //C W L D CR LF EOF LF
        public static readonly byte[] cwls4_header = new byte[8] { 67, 87, 76, 83, 13, 10, 26, 10 }; //C W L S CR LF EOF LF
        public static readonly byte[] cwls_header = new byte[4] { 67, 87, 76, 83 }; //CWLS > 4 header
        public static readonly byte[] cwll_header = new byte[4] { 67, 87, 76, 76 }; //CWLL

        public static readonly string tinyurl_api = "http://tinyurl.com/api-create.php?url=";
        public static readonly string tinyurl = "http://tinyurl.com/";

        public static readonly string NA = "N/A";

        public static readonly string http = "http://";
        public static readonly string https = "https://";
        public static readonly string ftp = "ftp://";
    }
}
