namespace Soteo.Shared.Extensions;

public static class EnumExtensions
{
    extension (Enum self)
    {
        public static TEnum[] GetValues<TEnum>() where TEnum : Enum =>
            Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
    }
}