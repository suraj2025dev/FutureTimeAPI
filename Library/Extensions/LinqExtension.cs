using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Library.Extensions
{
    public static class LinqExtension
    {

        /// <summary>
        /// Will reverse array on the basis of position.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<T> ReverseIndex<T>(this IEnumerable<T> array)
        {
            var arrayList = (List<T>)array;
            List<T> newList = new List<T>();
            for (int i = arrayList.Count - 1; i >= 0; i--)
            {
                newList.Add(arrayList[i]);
            }
            return newList;
        }


        /// <summary>
        /// Will help to order alpha numeric characters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string PadNumbers(this string input)
        {
            input = input == null ? "" : input;
            return Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
        }

    }
}
