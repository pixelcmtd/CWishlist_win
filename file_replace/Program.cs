using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace file_replace
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for CWishlist to exit.");
            Thread.Sleep(50);
            while (Process.GetProcessesByName("CWishlist_win.exe").Length > 1)
                Thread.Sleep(10);
            Console.WriteLine("Starting download.");
            Console.Write("0% done.");
            WebClient wc = new WebClient();
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler((s, e) =>
            {
                Console.SetCursorPosition(0, 1);
                Console.Write("{0}% done.", e.ProgressPercentage);
            });
            wc.DownloadFileAsync(new Uri(args[0]), args[1]);
            while (wc.IsBusy)
                Thread.Sleep(1);
            Process.Start(args[2]);
        }
    }
}
