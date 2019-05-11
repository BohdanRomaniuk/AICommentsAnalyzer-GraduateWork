using HtmlAgilityPack;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace parser.Helpers
{
    public static class StringHelper
    {
        public static string GetPropertyValueByLabel(string key, HtmlNodeCollection labels, HtmlNodeCollection infos, string def = "")
        {
            var currentDesc = labels.FirstOrDefault(l => l.InnerText == key);
            return currentDesc != null ? infos[labels.IndexOf(currentDesc)]?.InnerText ?? def : def;
        }

        public static DateTime GetDateTime(this string date)
        {
            if (date.Contains("Сьогодні") || date.Contains("Вчора"))
            {
                var time = Regex.Match(date, "[0-9]{2}:[0-9]{2}")?.Value?.Split(':');
                int hours = time != null ? Convert.ToInt32(time[0]) : 0;
                int minutes = time != null ? Convert.ToInt32(time[1]) : 0;
                if (date.Contains("Вчора"))
                {
                    return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hours, minutes, 0).AddDays(-1);
                }
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hours, minutes, 0);
            }
            CultureInfo culture = new CultureInfo("uk-UA");
            if (DateTime.TryParse(date, culture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return DateTime.Now;
        }
    }
}
