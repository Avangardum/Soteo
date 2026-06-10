namespace Soteo.Core.Shared.Dto.Snapshots;

public static class Delta
{
    public static Delta<T> Between<T>(T from, T to) => Equals(from, to) ? Delta<T>.Unchanged : to;
}

public readonly record struct Delta<T>
{
    public static readonly Delta<T> Unchanged = default;
    
    public Delta(T newValue)
    {
        HasChanged = true;
        NewValue = newValue;
    }
    
    public bool HasChanged { get; }
    public T NewValue => HasChanged ? field : throw new InvalidOperationException("Value has not changed");

    public static implicit operator Delta<T>(T newValue) => new(newValue);
}
