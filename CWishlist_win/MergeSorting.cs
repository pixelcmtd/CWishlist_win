using System.Collections.Generic;
using System.Linq;

namespace CWishlist_win
{
    class MergeSorting
    {
        public static Item[] MergeSort(Item[] unsorted) => MergeSort(new List<Item>(unsorted)).ToArray().Reverse().ToArray();

        static List<Item> MergeSort(List<Item> unsorted)
        {
            if (unsorted.Count < 2)
                return unsorted;

            List<Item> left = new List<Item>();
            List<Item> right = new List<Item>();
            int middle = unsorted.Count / 2;

            for (int i = 0; i < middle; i++)
                left.Add(unsorted[i]);

            for (int i = middle; i < unsorted.Count; i++)
                right.Add(unsorted[i]);

            left = MergeSort(left);
            right = MergeSort(right);
            return Merge(left, right);
        }

        static List<Item> Merge(List<Item> left, List<Item> right)
        {
            List<Item> result = new List<Item>();

            while (left.Count > 0 || right.Count > 0)
            {
                if (left.Count > 0 && right.Count > 0)
                {
                    if (left.First() <= right.First())
                    {
                        result.Add(left.First());
                        left.Remove(left.First());
                    }
                    else
                    {
                        result.Add(right.First());
                        right.Remove(right.First());
                    }
                }
                else if (left.Count > 0)
                {
                    result.Add(left.First());
                    left.Remove(left.First());
                }
                else if (right.Count > 0)
                {
                    result.Add(right.First());
                    right.Remove(right.First());
                }
            }
            return result;
        }
    }
}
