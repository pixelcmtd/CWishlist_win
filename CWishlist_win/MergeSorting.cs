using System.Collections.Generic;
using System.Linq;

namespace CWishlist_win
{
    class MergeSorting
    {
        public static Item[] MergeSort(Item[] unsorted) => MergeSort(new List<Item>(unsorted)).ToArray().Reverse().ToArray();

        static List<Item> MergeSort(List<Item> u)
        {
            if (u.Count < 2)
                return u;

            List<Item> l = new List<Item>();
            List<Item> r = new List<Item>();
            int m = u.Count / 2;

            for (int i = 0; i < m; i++)
                l.Add(u[i]);

            for (int i = m; i < u.Count; i++)
                r.Add(u[i]);

            l = MergeSort(l);
            r = MergeSort(r);
            return Merge(l, r);
        }

        static List<Item> Merge(List<Item> l, List<Item> r)
        {
            List<Item> res = new List<Item>();

            while (l.Count > 0 || r.Count > 0)
            {
                if (l.Count > 0 && r.Count > 0)
                    if (l.First() <= r.First())
                    {
                        res.Add(l.First());
                        l.Remove(l.First());
                    }
                    else
                    {
                        res.Add(r.First());
                        r.Remove(r.First());
                    }
                else if (l.Count > 0)
                {
                    res.Add(l.First());
                    l.Remove(l.First());
                }
                else if (r.Count > 0)
                {
                    res.Add(r.First());
                    r.Remove(r.First());
                }
            }
            return res;
        }
    }
}
