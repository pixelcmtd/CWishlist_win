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
            bool in_arr = false;
            while (r.Read())
            {
                string val_name = r.Value as string;
                if (r.TokenType == JsonToken.StartObject && in_arr)
                {
                    string name = null;
                    string url = null;
                    int id = -1;
                    long time = -1;
                    while (r.Read() && r.TokenType != JsonToken.EndObject)
                    {
                        val_name = r.Value as string;
                        Console.WriteLine(val_name);
                        if (r.TokenType == JsonToken.PropertyName)
                        {
                            if (val_name == "date_added")
                            {
                                time = long.Parse(r.ReadAsString());
                            }
                            else if (val_name == "id")
                            {
                                id = int.Parse(r.ReadAsString());
                            }
                            else if (val_name == "name")
                            {
                                name = r.ReadAsString();
                            }
                            else if (val_name == "url")
                            {
                                url = r.ReadAsString();
                            }
                        }
                        else if (r.TokenType == JsonToken.StartObject)
                        {
                            r.Read();
                            r.Read();
                            r.Read();
                        }
                    }
                    bms.Add(new bookmark(name, url, id, time));
                }
                else if (r.TokenType == JsonToken.StartArray)
                {
                    in_arr = true;
                }
                else if (r.TokenType == JsonToken.EndArray)
                {
                    in_arr = false;
                }
                else if (r.TokenType == JsonToken.PropertyName && val_name == "checksum")
                {
                    checksum = hex(r.ReadAsString());
                }
            }
            return new bookmarks(new checksum(checksum_type.MD5, checksum), bms.ToArray());
        }
    }
}
