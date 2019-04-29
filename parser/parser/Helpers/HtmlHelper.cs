using System;
using System.Text.RegularExpressions;

namespace parser.Helpers
{
    public static class HtmlHelper
    {
        public static string StripHtml(string html)
        {
            return Regex.Replace(html, "<.*?>", String.Empty);
        }
    }
}
