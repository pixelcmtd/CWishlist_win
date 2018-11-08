using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override string ToString()
        {
            return "{\"" + name + "\",\"" + url + "\"," + id + "," + time + "}";
        }
    }
}
