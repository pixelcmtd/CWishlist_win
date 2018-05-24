using System.Collections.Generic;

namespace CWishlist_win
{
    class LanguageProvider
    {
        public static LANG selected { get; set; } = LANG.EN;

        public static LANG get_lang(byte id) => id == 0x00 ? LANG.EN : LANG.DE;

        public static byte get_id(LANG lang) => lang == LANG.EN ? (byte)0x00 : (byte)0x01;

        static Dictionary<string, dynamic> en_vals = new Dictionary<string, dynamic>()
        {
            {"misc.changelog", new string[]
            {
            "CWishlist by chrissx @ chrissx Media Inc. Changelog:",
            "",
            "Version 4.1.0:",
            "-Many under-the-hood improvements",
            "-A GitHub-repo (https://github.com/chrissxYT/CWishlist_win)",
            "",
            "Version 4.0.1:",
            "-Day one patch for a file extention bug in 4.0.0",
            "",
            "Version 4.0.0:",
            "-Introduction of the new CWishlistUncde v1 standard (a technical explanation will come up at some point)",
            "",
            "Version 3.3.0:",
            "-Added background color setting (with saving)",
            "",
            "Version 3.2.1:",
            "-Added saving of window size",
            "",
            "Version 3.2.0:",
            "-Added StackOverflow-warning",
            "-Added arrow-key use for switching through the wishlist",
            "",
            "Version 3.1.0:",
            "-Added searching for Wishlist-items",
            "",
            "Version 3.0.0:",
            "-Added merge-sorting for Wishlist-items",
            "-Added most of the language support for German",
            "-Using tinyurl-API for URL-shortening",
            "",
            "Version 2.3.2:",
            "-Fixed error with listbox-pointer being out-of-bounds",
            "-Kinda passively fixed backup-prompt showing up all the time",
            "-Fixed \"Open all\"-button not being aligned correctly when resizing",
            "",
            "Version 2.3.1:",
            "-Added this wonderful changelog",
            "-Fixed deadly bug in the recents-file"
            }
            },
            {"prompt.restore_backup", "The program was exited unexpectedly the last time, do you want to restore the backup?" },
            {"caption.restore_backup", "Ya really want 2 losé?" },
            {"prompt.close", "All your unsaved changes will be deleted, do you really want to close?" },
            {"caption.close", "Ya really want 2 closé?" },
            {"prompt.new", "All your unsaved changes will be deleted, do you really want to create a new file without saving?" },
            {"caption.new", "Ya really want 2 deleté?" },
            {"prompt.open", "All your unsaved changes will be deleted, do you really want to load another file without saving?" },
            {"caption.open", "Ya really want 2 open othr filé?" },
            {"prompt.stackoverflow", "Your stack-size is really close to the limit, please be sure that you always save your wishlist to not lose any of your contents." },
            {"caption.stackoverflow", "Ya really want 2 hav that hi stakk sizé?" },
            {"prompt.switch_lang", "Do you want to switch to German as your language?" },
            {"caption.switch_lang", "Ya really want 2 switch 2 Germé?" }
        };

        static Dictionary<string, dynamic> de_vals = new Dictionary<string, dynamic>()
        {
            {"misc.changelog", new string[]
            {
            "CWishlist von chrissx bei chrissx Media Inc. Changelog:",
            "",
            "Version 4.1.0:",
            "-Sehr viele Under-The-Hood-Verbesserungen",
            "-Ein GitHub-repository (https://github.com/chrissxYT/CWishlist_win)",
            "",
            "Version 4.0.1:",
            "-Day one patch für einen Dateiendungsbug in 4.0.0",
            "",
            "Version 4.0.0:",
            "-Der neue CWishlistUncde v1 Standart wurde eingeführt (eine technische Erklärung wird irgendwann folgen)",
            "",
            "Version 3.3.0:",
            "-Hintergrundfarbeneinstellung hinzugefügt (wird gespeichert)",
            "",
            "Version 3.2.1:",
            "-Die Fenstergröße wird nun gespeichert",
            "",
            "Version 3.2.0:",
            "-StackOverflow-Warnung hinzugefügt",
            "-Die Pfeiltasten können jetzt benutzt werden, um durch die Liste zu gehen",
            "",
            "Version 3.1.0:",
            "-Suchfunktion für Wunschlisten-Items hinzugefügt",
            "",
            "Version 3.0.0:",
            "-Merge-sorting für Wunschlisten-Items hinzugefügt",
            "-Der größte Teil der Unterstützung der deutschen Sprache wurde erledigt",
            "-Tinyurl wird jetzt für URL-Verkürzung verwendet",
            "",
            "Version 2.3.2:",
            "-Fehler mit einem out-of-bounds listbox-index",
            "-Die Backup-Prompt wurde bei jedem Start angezeigt, sollte jetzt nicht mehr passieren",
            "-Der \"Open all\"-Button wurde beim Verändern der Größe nicht richtig platziert",
            "",
            "Version 2.3.1:",
            "-Dieser wundervolle Changelog wurde hinzugefügt!!",
            "-Ein tödlicher Bug mit der recents-Datei wurde behoben."
            }
            },
            {"prompt.restore_backup", "Das Programm wurde letztes mal unerwartet beendet, wollen Sie das Backup wiederherstellen?" },
            {"caption.restore_backup", "Du willschd weagli verliereñ?" },
            {"prompt.close", "Alle ungespeicherten Veränderungen werden verworfen, wollen Sie das Programm wirklich schließen?" },
            {"caption.close", "Du willschd weagli schließeñ?" },
            {"prompt.new", "Alle ungespeicherten Veränderungen werden verworfen, wollen Sie wirklich eine neue Datei erstellen, ohne zu speichern?" },
            {"caption.new", "Du willschd weagli löscheñ?" },
            {"prompt.open", "Alle ungespeicherten Veränderungen werden verworfen, wollen Sie wirklich eine andere Datei öffnen, ohne zu speichern?" },
            {"caption.open", "Du willschd weagli ne andre Dadei öffneñ?" },
            {"prompt.stackoverflow", "Ihre Stack-Größe ist nahe am Limit, bitte versichern Sie sich, Ihre Liste immer zu speichern." },
            {"caption.stackoverflow", "Du willschd weagli nen Schdägg-Ofafloo verursacheñ?" },
            {"prompt.switch_lang", "Wollen Sie Englisch als Sprache auswählen?" },
            {"caption.switch_lang", "Du willschd weagli Éngísch wähleñ?" }
        };

        public static dynamic get_translated(string name) => selected == LANG.DE ? (de_vals.ContainsKey(name) ? de_vals[name] : name) : (en_vals.ContainsKey(name) ? en_vals[name] : name);
    }

    enum LANG
    {
        DE, EN
    }
}
