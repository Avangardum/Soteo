using System.Numerics;

namespace Soteo.Core.Gameplay.Dto;

public sealed class ExtraData
{
    private readonly object?[] _values;
    
    private ExtraData(int count)
    {
        _values = new object[count];
    }

    public void Set<T>(Key<T> key, T value)
    {
        _values[key.Index] = value;
    }

    public T Get<T>(Key<T> key)
    {
        object? value = _values[key.Index];
        if (value == null)
        {
            if (key.HasDefault)
            {
                // Null forgiving operator is used instead of Required because default value can be null,
                // but only for nullable T
                return key.Default!;
            }
            throw new InvalidOperationException("Value is not initialized");
        }
        return (T)value;
    }
    
    public class Builder
    {
        private int _count;
        
        public Key<int> AddIntLateInit() => new(_count++);
        public Key<long> AddLongLateInit() => new(_count++);
        public Key<double> AddDoubleLateInit() => new(_count++);
        public Key<Guid> AddGuidLateInit() => new(_count++);
        public Key<Vector2> AddVector2LateInit() => new(_count++);
        
        public Key<int> AddIntWithDefault(int defaultValue = 0) => new(_count++, defaultValue);
        public Key<long> AddLongWithDefault(long defaultValue = 0) => new(_count++, defaultValue);
        public Key<double> AddDoubleWithDefault(double defaultValue = 0) => new(_count++, defaultValue);
        public Key<Guid> AddGuidWithDefault(Guid defaultValue = default) => new(_count++, defaultValue);
        public Key<Vector2> AddVector2WithDefault(Vector2 defaultValue = default) => new(_count++, defaultValue);
        
        public Key<int?> AddNullableInt() => new(_count++, null);
        public Key<long?> AddNullableLong() => new(_count++, null);
        public Key<double?> AddNullableDouble() => new(_count++, null);
        public Key<Guid?> AddNullableGuid() => new(_count++, null);
        public Key<Vector2?> AddNullableVector2() => new(_count++, null);
        
        public Key<int?> AddNullableIntWithDefault(int defaultValue) => new(_count++, defaultValue);
        public Key<long?> AddNullableLongWithDefault(long defaultValue) => new(_count++, defaultValue);
        public Key<double?> AddNullableDoubleWithDefault(double defaultValue) => new(_count++, defaultValue);
        public Key<Guid?> AddNullableGuidWithDefault(Guid defaultValue) => new(_count++, defaultValue);
        public Key<Vector2?> AddNullableVector2WithDefault(Vector2 defaultValue) => new(_count++, defaultValue);
        
        public ExtraData Build() => new(_count);
    }
    
    public sealed class Key<T>
    {
        internal int Index { get; }
        internal bool HasDefault { get; }
        internal T? Default { get; } 
        
        internal Key(int index)
        {
            Index = index;
        }
        
        internal Key(int index, T defaultValue)
        {
            Index = index;
            HasDefault = true;
            Default = defaultValue;
        }
    }
}
