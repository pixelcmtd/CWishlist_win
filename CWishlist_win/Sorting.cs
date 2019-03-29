using static binutils.io;

namespace CWishlist_win
{
    public static class Sorting
    {
        public static void quicksort(int left, int right, ref Item[] data)
        {
            dbg("[QuickSort]Sorting...");
            qs(left, right, ref data);
            dbg("[QuickSort]Done sorting.");
        }

        static void qs(int left, int right, ref Item[] data)
        {
            if (left < right)
            {
                int i = pivot(left, right, ref data);
                quicksort(left, i - 1, ref data);
                quicksort(i + 1, right, ref data);
            }
        }

        static int pivot(int left, int right, ref Item[] data)
        {
            dbg("[QuickSort]Pivoting...");
            int i = left;
            int j = right - 1;
            int pivot = data[right];
            while (i < j)
            {
                while (data[i] <= pivot && i < right) i += 1;
                while (data[j] >= pivot && j > left) j -= 1;
                if (i < j)
                {
                    Item z = data[i];
                    data[i] = data[j];
                    data[j] = z;
                }
            }
            if (data[i] > pivot)
            {
                Item z = data[i];
                data[i] = data[right];
                data[right] = z;
            }
            dbg("[QuickSort]Pivoted.");
            return i;
        }
    }
}
