using System.Text.RegularExpressions;

namespace Soteo.Shared.Extensions;

public static class StringExtensions
{
    extension (string self)
    {
        public string ReplaceRegex(string pattern, string replacement) =>
            Regex.Replace(self, pattern, replacement);
    }
}