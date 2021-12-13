using System.Text.RegularExpressions;

namespace XSLTransforms
{
    public static class StringExtensions
    {
        private static readonly Regex SpaceRegexp = new Regex(@"\s+", RegexOptions.Compiled);

        public static string RemoveNewLinesAndConsecutiveSpaces(this string value) => SpaceRegexp.Replace(value, " ");
    }
}
