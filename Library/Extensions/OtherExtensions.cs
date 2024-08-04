using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Extensions
{
    public static class OtherExtensions
    {
        public static bool IsBetween<T>(this T item, T start, T end)
        {
            return Comparer<T>.Default.Compare(item, start) >= 0
                && Comparer<T>.Default.Compare(item, end) <= 0;
        }

        public static string SafeSubstring(this string orig, int length)
        {
            return orig.Substring(0, orig.Length >= length ? length : orig.Length);
        }

        public static int ParseIntOrZero(this string s)
        {
            int.TryParse(s, out int result);
            return result;
        }
        public static string ReverseString(this string orig)
        {
            char[] charArray = orig.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        public static decimal ToFixed(this decimal val, int decimals)
        {
            return Math.Round(val, decimals);
        }
    }
}
