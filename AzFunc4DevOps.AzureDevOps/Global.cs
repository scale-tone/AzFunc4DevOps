using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AzFunc4DevOps.AzureDevOps
{
    public static class Global 
    {
        public const string FunctionPrefix = "AzFunc4DevOps";

        public static Random Rnd = new Random();

        public static Task DelayForAboutASecond()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(Rnd.Next(500, 1500)));
        }

        public static string GetMd5Hash(this string str)
        {
            using(var hash = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(str);

                var hashBytes = hash.ComputeHash(bytes);
                var hashString = string.Join("", hashBytes.Select(b => b.ToString("X2")));

                return hashString;
            }
        }

        public static IEnumerable<List<T>> ToBatches<T>(this IEnumerable<T> items, int batchSize)
        {
            var batch = new List<T>();

            foreach(var item in items)
            {
                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    yield return batch;
                    batch = new List<T>();
                }
            }

            if (batch.Count > 0)
            {
                yield return batch;
            }
        }

        public static bool In<T>(this T val, params T[] values)
        {
            return values.Contains(val);
        }

        public static int FindIndex<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}