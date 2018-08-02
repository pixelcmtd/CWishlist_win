using System;

namespace CWishlist_win
{
    class Sorting
    {
        public static Item[] merge_sort_items(Item[] u)
        {
            GC.TryStartNoGCRegion(8 * 1024 * 1024);
            Item[] i = m(u);
            GC.EndNoGCRegion();
            return i;
        }

        public static Item[] m(Item[] u)
        {
            if (u.Length < 2)
                return u;

            int mid = u.Length / 2; //Structs are stored on the stack, so there is a 1 MiB limit to them, so 32bit length is ok
            Item[] l = new Item[mid];
            Item[] r = new Item[u.Length % 2 == 1 ? mid + 1 : mid];

            for (int i = 0; i < mid; i++)
                l[i] = u[i];

            for (int i = mid; i < u.Length; i++)
                r[i - mid] = u[i];

            l = m(l);
            r = m(r);
            return m(l, r);
        }

        static Item[] m(Item[] l, Item[] r)
        {
            Item[] res = new Item[l.Length + r.Length];
            int res_idx = 0;
            int li = 0;
            int ri = 0;

            while (l.Length > 0 || r.Length > 0)
            {
                if (l.Length > 0 && r.Length > 0)
                    if (l[0] >= r[0])
                    {
                        res[res_idx] = l[li];
                        res_idx++;
                        li++;
                    }
                    else
                    {
                        res[res_idx] = r[ri];
                        res_idx++;
                        ri++;
                    }
                else if (l.Length > 0)
                {
                    res[res_idx] = l[li];
                    res_idx++;
                    li++;
                }
                else if (r.Length > 0)
                {
                    res[res_idx] = r[ri];
                    res_idx++;
                    ri++;
                }
            }
            return res;
        }
    }
}
