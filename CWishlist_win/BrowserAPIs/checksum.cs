namespace CWishlist_win
{
    struct checksum
    {
        public checksum_type type;
        public byte[] bytes;

        public checksum(checksum_type type, byte[] bytes)
        {
            this.type = type;
            this.bytes = bytes;
        }

        public string dbgstr(bool beautify = false, string tabs = null, string single_tab = null)
        {
            if (beautify)
            {
                return "{\n" + tabs + single_tab + type + ",\n" + tabs + single_tab +
                    CLinq.hex(bytes) + "\n" + tabs + "}";
            }
            else
            {
                return "{" + type + "," + CLinq.hex(bytes) + "}";
            }
        }
    }
}
