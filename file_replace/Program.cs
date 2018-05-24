using System;
using System.Net;
using System.Threading;

namespace file_replace
{
    class Program
    {
        static void Main(string[] args)
        {
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
        }
    }
}
