using CWishlist_win;
using System;
using System.Diagnostics;
using System.IO;

namespace cwla_benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Source WL: ");
            string f = Console.ReadLine();
            WL wl = IO.load(f);
            Console.Write("A: ");
            string a = Console.ReadLine();
            Console.Write("B: ");
            string b = Console.ReadLine();
            Console.Write("C: ");
            string c = Console.ReadLine();
            string t0 = Path.GetTempFileName();
            Console.Write("PAQ1: ");
            string paq1 = Console.ReadLine();
            Console.Write("PAQ8P: ");
            string paq8p = Console.ReadLine();
            Console.Write("PAQ9A: ");
            string paq9a = Console.ReadLine();
            Stream s = File.Open(t0, FileMode.Create, FileAccess.Write);
            foreach (Item i in wl)
                i.write(s);
            s.Close();
            Process.Start(paq1, $"\"{a}\" \"{t0}\"");
            Process.Start(paq8p, $"\"{b}\" \"{t0}\"").WaitForExit();
            File.Move(b + ".paq8p", b);
            Process.Start(paq9a, $"a \"{c}\" \"{t0}\"");
        }
    }
}
