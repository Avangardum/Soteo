using System.Text.RegularExpressions;

namespace Soteo.Shared.Extensions;

public static class StringExtensions
{
    extension (string self)
    {
        public string ReplaceRegex(string pattern, string replacement) =>
            Regex.Replace(self, pattern, replacement);
        
        public string ReplaceRegex(string pattern, MatchEvaluator evaluator) =>
            Regex.Replace(self, pattern, evaluator);
        
        public string PascalCaseToCapitalizedText() =>
            self.ReplaceRegex("(?<=.)[A-Z]", " $0");
        
        public string PascalCaseToSnakeCase() =>
            self.ReplaceRegex("(?<=.)[A-Z]", "_$0").ToLower();
    }
}