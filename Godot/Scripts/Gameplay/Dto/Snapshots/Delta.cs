namespace Soteo.Gameplay.Dto.Snapshots;

public readonly record struct Delta<T>
{
    private Delta(T newValue)
    {
        HasChanged = true;
        NewValue = newValue;
    }
    
    public bool HasChanged { get; }
    public T NewValue => HasChanged ? field : throw new InvalidOperationException("Value has not changed");

    public static implicit operator Delta<T>(T newValue) => new(newValue);
}