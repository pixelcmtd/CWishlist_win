using static System.GC;

namespace CWishlist_win
{
    class Sorting
    {
        public static Item[] merge_sort_items(Item[] u)
        {
            TryStartNoGCRegion(15 * 1024 * 1024 + 1024 * 1024 * 127, 127 * 1024 * 1024, true);
            Item[] i = s(u);
            EndNoGCRegion();
            return i;
        }

        static Item[] s(Item[] u)
        {
            if (u.Length < 2)
                return u;

            long m = u.LongLength / 2;
            Item[] l = new Item[m];
            Item[] r = new Item[u.Length % 2 == 1 ? m + 1 : m];

            for (long i = 0; i < m; i++)
                l[i] = u[i];

            for (long i = m; i < u.Length; i++)
                r[i - m] = u[i];

            l = s(l);
            r = s(r);
            return s(l, r);
        }

        static Item[] s(Item[] l, Item[] r)
        {
            Item[] a = new Item[l.LongLength + r.LongLength];
            long ai = 0;
            long li = 0;
            long ri = 0;
            long ll = l.LongLength;
            long rl = r.LongLength;
            bool ld;
            bool rd;

            while ((ld = ll - li > 0) || rl - ri > 0)
            {
                rd = rl - ri > 0;
                if (ld && rd)
                    if (l[li] >= r[ri])
                    {
                        a[ai] = l[li];
                        ai++;
                        li++;
                    }
                    else
                    {
                        a[ai] = r[ri];
                        ai++;
                        ri++;
                    }
                else if (ld)
                {
                    a[ai] = l[li];
                    ai++;
                    li++;
                }
                else if (rd)
                {
                    a[ai] = r[ri];
                    ai++;
                    ri++;
                }
            }
            return a;
        }
    }
}
