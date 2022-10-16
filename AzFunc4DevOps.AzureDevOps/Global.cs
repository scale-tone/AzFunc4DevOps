using System;
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
    }
}