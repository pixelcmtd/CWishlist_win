using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static CWishlist_win.CLinq;

namespace CWishlist_win
{
    static class Chrome
    {
        public static bookmarks parse(string json_file)
        {
            JsonTextReader r = new JsonTextReader(new StreamReader(json_file));
            List<bookmark> bms = new List<bookmark>();
            byte[] checksum = null;
            while (r.Read())
            {
                if (r.TokenType == JsonToken.PropertyName)
                {
                    string val_name = r.Value as string;
                    if (val_name == "checksum")
                    {
                        checksum = hex(r.ReadAsString());
                    }
                }
                Console.Write(r.ValueType);
                Console.Write(" ");
                Console.Write(r.TokenType);
                Console.Write(" ");
                Console.WriteLine(r.Value);
            }
            return new bookmarks(checksum, bms.ToArray());
        }
    }
}
