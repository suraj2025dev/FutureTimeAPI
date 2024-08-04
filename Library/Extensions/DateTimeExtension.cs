using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Extensions
{
    public static class DateTimeExtension
    {
        //ConvertDateTime toggles date time between date and string format.
        public static DateTime ConvertDateTime(this string dateTimeString)
        {
            return DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss",//"2009-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff"
                                       System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string ConvertDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static DateTime _AddDays(this DateTime dateTime,int days)
        {
            return dateTime.AddDays(days);
        }

        public static string _AddDays(this string dateTimeString, int days)
        {
            var dateTime=dateTimeString.ConvertDateTime().AddDays(days);

            return dateTime._AddDays(days).ConvertDateTime();
        }

        /// <summary>
        /// Used to convert UTC datetime from API request to local datetime.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime ToLocal(this DateTime datetime)
        {
            if (datetime.Kind == DateTimeKind.Utc)
                return DateTime.SpecifyKind(datetime, DateTimeKind.Utc).ToLocalTime();
            else
                return datetime;
        }

        /// <summary>
        /// Used to convert UTC datetime from API request to local datetime.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime? ToLocal(this DateTime? datetime1)
        {
            if (datetime1 == null) return null;
            DateTime datetime = (DateTime)datetime1;
            if (datetime.Kind == DateTimeKind.Utc)
                return DateTime.SpecifyKind(datetime, DateTimeKind.Utc).ToLocalTime();
            else
                return datetime;
        }
    }
}
