using System.Collections.Generic;
using System.Xml;

namespace CWishlist_win
{
    class LanguageProvider
    {
        public static lang selected { get; set; } = new lang("en", "english");

        public static Dictionary<lang, Dictionary<string, dynamic>> langs = new Dictionary<lang, Dictionary<string, dynamic>>();

        public static dynamic get_translated(string name) => langs[selected][name];

        public static lang get_lang(string code) => langs.Keys.where((l) => l.code == code);

        public static void load_lang_xml(string file)
        {
            lang l = new lang("na-lang", "Language not available.");
            Dictionary<string, dynamic> translations = new Dictionary<string, dynamic>();
            XmlReader xml = XmlReader.Create(file);
            while (xml.Read())
                if(xml.NodeType == XmlNodeType.Element)
                    if (xml.Name == "lang")
                        l = new lang(xml.GetAttribute("code"), xml.GetAttribute("name"));
                    else if (xml.Name == "translation")
                    {
                        string name = xml.GetAttribute("name");
                        string type = xml.GetAttribute("type");
                        string val = xml.GetAttribute("value");
                        dynamic v = "NOPE...NO TRANSLATION HERE";
                        switch (type)
                        {
                            case "str_arr": v = val.Split('\\'); break;
                            case "str": v = val; break;
                            case "int_arr": v = val.Split('\\').parse_ints(); break;
                            case "int": v = int.Parse(val); break;
                            case "uint_arr": v = val.Split('\\').parse_uints(); break;
                            case "uint": v = uint.Parse(val); break;
                            case "short_arr": v = val.Split('\\').parse_shorts(); break;
                            case "short": v = short.Parse(val); break;
                            case "ushort_arr": v = val.Split('\\').parse_ushorts(); break;
                            case "ushort": v = ushort.Parse(val); break;
                            case "long_arr": v = val.Split('\\').parse_longs(); break;
                            case "long": v = long.Parse(val); break;
                            case "ulong_arr": v = val.Split('\\').parse_ulongs(); break;
                            case "ulong": v = ulong.Parse(val); break;
                            case "byte_arr": v = val.Split('\\').parse_bytes(); break;
                            case "byte": v = byte.Parse(val); break;
                            case "sbyte_arr": v = val.Split('\\').parse_sbytes(); break;
                            case "sbyte": v = sbyte.Parse(val); break;
                            case "decimal_arr": v = val.Split('\\').parse_decimals(); break;
                            case "decimal": v = decimal.Parse(val); break;
                            case "bool_arr": v = val.Split('\\').parse_bools(); break;
                            case "bool": v = bool.Parse(val); break;
                            case "char_arr": v = val.Split('\\').parse_chars(); break;
                            case "char": v = char.Parse(val); break;
                            default: v = $"Bad language translation. (lang: {l.code}/{l.name}, name: {name}, type: {type}, raw value: {val})"; break;
                        }
                        translations.Add(name, v);
                    }
            langs.Add(l, translations);
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

        public override bool Equals(object obj)
        {
            return obj is lang ? code == ((lang)obj).code && name == ((lang)obj).name : false;
        }

        public bool Equals(lang lang)
        {
            return lang.code == code && lang.name == name;
        }

        public override int GetHashCode()
        {
            return code.GetHashCode();
        }
    }
}
