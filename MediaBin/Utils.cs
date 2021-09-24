using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBin
{
    public class Utils
    {
        public static string GenerateRandomAlphaString(int length = 5)
        {
            Random rng = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rng.Next(s.Length)]).ToArray());
        }

        public static string FriendlySize(long bytes)
        {
            var output = (float)bytes;

            var prefixes = new string[]
            {
                "Bytes",
                "KiB",
                "MiB",
                "GiB",
            };
            int i;
            for (i = 0; i < prefixes.Length; i++)
            {
                if (output < 1024)
                    break;

                output /= 1024;
            }

            return string.Format("{0:0.##} {1}", output, prefixes[i]);
        }
    }
}
