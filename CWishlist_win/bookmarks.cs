namespace CWishlist_win
{
    struct bookmarks
    {
        public byte[] checksum; //MD5 in Chrome (https://chromium.googlesource.com/chromium/src.git/+/master/components/bookmarks/browser/bookmark_codec.cc)
        public bookmark[] marks;

        public bookmarks(byte[] checksum, bookmark[] marks)
        {
            this.checksum = checksum;
            this.marks = marks;
        }
    }
}
