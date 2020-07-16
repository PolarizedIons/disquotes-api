using System.Text.RegularExpressions;

namespace QuotesApi.Extentions
{
    public static class StringExtention
    {
        public static string SplitTitleCase(this string value)
        {
            return Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
        }
    }
}
