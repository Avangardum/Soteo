using System.Text.RegularExpressions;

namespace Soteo.Shared.Extensions;

public static class RegexExtensions
{
    extension (Group self)
    {
        public string? NullableValue => self.Success ? self.Value : null;
    }
}