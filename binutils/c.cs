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
    }
}
