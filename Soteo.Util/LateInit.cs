namespace Soteo.Util;

/// <summary>
/// Immutable value that must be initialized before using,
/// but can't be initialized in a constructor or initializer.
/// </summary>
public sealed class LateInit<T>
{
    private bool _hasValue;
    
    public T Value
    {
        // Null forgiving operator is used instead of Required since value can actually be null,
        // but null can only be returned if T is nullable.
        get => _hasValue ? field! : throw new InvalidOperationException("Value is not initialized");
        set
        {
            if (_hasValue)
                throw new InvalidOperationException("Value is already initialized");
            field = value;
            _hasValue = true;
        }
    }
    
    public static implicit operator T(LateInit<T> self) => self.Value;
}
