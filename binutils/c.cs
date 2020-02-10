using System.IO;

namespace binutils
{
    public static class c
    {
        public static void arrcpy<T>(T[] arr1, T[] arr2, long len)
        {
            for (long i = 0; i < len; i++)
                arr2[i] = arr1[i];
        }

        //file compare
        public static bool fcmp(FileStream stream1, FileStream stream2)
        {
            if (stream1.Length - stream1.Position != stream2.Length - stream2.Position)
                return false;
            int i;
            while ((i = stream1.ReadByte()) != -1 && i == stream2.ReadByte()) ;
            return i == -1;
        }

        //file compare
        public static bool fcmp(string f1, string f2)
        {
            FileStream s1 = File.Open(f1, FileMode.Open, FileAccess.Read);
            FileStream s2 = File.Open(f2, FileMode.Open, FileAccess.Read);
            bool b = fcmp(s1, s2);
            s1.Close();
            s2.Close();
            return b;
        }

        //fastcharactercontains
        public static bool fccontains(string s, char c)
        {
            foreach (char d in s)
                if (c == d)
                    return true;
            return false;
        }
    }
}
