namespace Soteo.Core.Serialization;

public static class EnumValidator<T> where T : Enum
{
    private static readonly HashSet<T>? Values;

    static EnumValidator()
    {
        if (typeof(T).HasAttribute<FlagsAttribute>() || typeof(T).HasAttribute<AllowAnonymousEnumValuesAttribute>())
            return;
        Values = [..Enum.GetValues<T>()];
    }

    public static bool IsValid(T value) => Values == null || Values.Contains(value);
}
