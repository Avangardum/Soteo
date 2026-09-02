namespace Soteo.Util.Extensions;

public static class EnumExtensions
{
    extension (Enum self)
    {
        public static TEnum[] GetValues<TEnum>() where TEnum : Enum =>
            Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
    }
    
    extension<T> (T self) where T : Enum
    {
        public static T Parse(string value) => (T)Enum.Parse(typeof(T), value);
    }
}
