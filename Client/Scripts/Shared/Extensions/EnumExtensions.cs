namespace Soteo.Shared.Extensions;

public static class EnumExtensions
{
    extension (Enum self)
    {
        public static TEnum[] GetValues<TEnum>() => Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
    }
}