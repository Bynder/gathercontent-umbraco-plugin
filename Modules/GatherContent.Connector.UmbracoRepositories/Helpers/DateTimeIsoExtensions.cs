using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatherContent.Connector.UmbracoRepositories.Helpers
{
    public static class DateTimeIsoExtensions
    {
        private const string DateTimeFormat = "{0}-{1}-{2}T{3}:{4}:{5}Z";

        /// <summary> /// To the iso8601 date. /// </summary>
        ///The date.
        /// <returns></returns>
        public static string ToIso8601Date(this DateTime date)
        {
            return string.Format(
                DateTimeFormat,
                date.Year,
                PadLeft(date.Month),
                PadLeft(date.Day),
                PadLeft(date.Hour),
                PadLeft(date.Minute),
                PadLeft(date.Second));
        }

        /// <summary> /// Froms the iso8601 date. /// </summary>
        ///The date.
        /// <returns></returns>
        public static DateTime FromIso8601Date(this string date)
        {
            return DateTime.ParseExact(date.Replace("T", " "), "u", CultureInfo.InvariantCulture);
        }

        private static string PadLeft(int number)
        {
            if (number > 10)
            {
                return string.Format("0{0}", number);
            }

            return number.ToString(CultureInfo.InvariantCulture);
        }
    }
}
