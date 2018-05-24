﻿using System.Collections.Generic;
using System.Xml;

namespace CWishlist_win
{
    class LanguageProvider
    {
        public static lang selected { get; set; } = new lang("en", "english");

        public static Dictionary<lang, Dictionary<string, dynamic>> langs = new Dictionary<lang, Dictionary<string, dynamic>>();

        public static dynamic get_translated(string name) => langs[selected][name];

        public static void load_lang_xml(string file)
        {
            lang lng;
            Dictionary<string, dynamic> translations = new Dictionary<string, dynamic>();
            XmlReader xml = XmlReader.Create(file);
            while (xml.Read())
                if (xml.NodeType == XmlNodeType.Element && xml.Name == "lang")
                    lng = new lang(xml.GetAttribute("code"), xml.GetAttribute("name"));
                else if (xml.NodeType == XmlNodeType.Element && xml.Name == "translation")
                {
                    string name = xml.GetAttribute("name");
                    string type = xml.GetAttribute("type");
                    string val = xml.GetAttribute("value");
                    dynamic value = -1;
                    switch (type)
                    {
                        case "str_arr": value = val.Replace("\r", "").Split('\n'); break;
                        case "str": value = val; break;
                        case "int_arr": value = val.Replace("\r", "").Split('\n').parse_int(); break;
                        case "int": value = int.Parse(val); break;
                        case "uint_arr": value = val.Replace("\r", "").Split('\n').parse_uint(); break;
                        case "uint": value = uint.Parse(val); break;
                        case "short_arr": value = val.Replace("\r", "").Split('\n').parse_short(); break;
                        case "short": value = short.Parse(val); break;
                        case "ushort_arr": value = val.Replace("\r", "").Split('\n').parse_ushort(); break;
                        case "ushort": value = ushort.Parse(val); break;
                        case "long_arr": value = val.Replace("\r", "").Split('\n').parse_long(); break;
                        case "long": value = long.Parse(val); break;
                        case "ulong_arr": value = val.Replace("\r", "").Split('\n').parse_ulong(); break;
                        case "ulong": value = ulong.Parse(val); break;
                        case "byte_arr": value = val.Replace("\r", "").Split('\n').parse_byte(); break;
                        case "byte": value = byte.Parse(val); break;
                        case "sbyte_arr": value = val.Replace("\r", "").Split('\n').parse_sbyte(); break;
                        case "sbyte": value = sbyte.Parse(val); break;
                        case "decimal_arr": value = val.Replace("\r", "").Split('\n').parse_decimal(); break;
                        case "decimal": value = decimal.Parse(val); break;
                        case "bool_arr": value = val.Replace("\r", "").Split('\n').parse_bool(); break;
                        case "bool": value = bool.Parse(val); break;
                        case "char_arr": value = val.Replace("\r", "").Split('\n').parse_char(); break;
                        case "char": value = char.Parse(val); break;
                        default: value = -1; break;
                    }
                    translations.Add(name, value);
                }
        }
    }

    struct lang
    {
        public lang(string code, string name)
        {
            this.code = code;
            this.name = name;
        }

        public string code;
        public string name;
    }
}
