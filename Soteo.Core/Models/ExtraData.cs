using System.Numerics;
using Soteo.Core.Exceptions;
using Soteo.Core.Interfaces;
using Soteo.Core.Services;
using Soteo.Core.Services.Serializers;

namespace Soteo.Core.Models;

/// <summary>
/// Serializable data container included in AbilityContext / StatusContext
/// for storing data specific to a concrete ability / status.
/// </summary>
public sealed class ExtraData(object?[] values, ISerializationHelper s)
{
    public void Set<T>(Key<T> key, T value)
    {
        values[key.Index] = value;
    }

    public T Get<T>(Key<T> key)
    {
        object? value = values[key.Index];
        if (value == null)
        {
            if (key.IsNullable) return default!;
            throw new InvalidOperationException("Value is not initialized");
        }
        return (T)value;
    }
    
    public void Serialize(Stream stream)
    {
        s.SerializeInt(values.Length, stream);
        foreach (object? value in values)
        {
            switch (value)
            {
                case null:
                    s.SerializeEnum(TypeCode.Null, stream);
                    break;
                case int i:
                    s.SerializeEnum(TypeCode.Int, stream);
                    s.SerializeInt(i, stream);
                    break;
                case long l:
                    s.SerializeEnum(TypeCode.Long, stream);
                    s.SerializeLong(l, stream);
                    break;
                case double d:
                    s.SerializeEnum(TypeCode.Double, stream);
                    s.SerializeDouble(d, stream);
                    break;
                case Guid g:
                    s.SerializeEnum(TypeCode.Guid, stream);
                    s.SerializeGuid(g, stream);
                    break;
                case Vector2 v:
                    s.SerializeEnum(TypeCode.Vector2, stream);
                    s.SerializeVector2(v, stream);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    public static ExtraData Deserialize(Stream stream)
    {
        var s = new SerializationHelper(TypeLocator.Empty);
        int count = s.DeserializeInt(stream);
        if (count < 0 || count > stream.Length - stream.Position)
            throw new BadSerializedDataException("Invalid ExtraData count");
        var values = new object?[count];
        for (int i = 0; i < count; i++)
        {
            var typeCode = s.DeserializeEnum<TypeCode>(stream);
            values[i] = typeCode switch
            {
                TypeCode.Null => null,
                TypeCode.Int => s.DeserializeInt(stream),
                TypeCode.Long => s.DeserializeLong(stream),
                TypeCode.Double => s.DeserializeDouble(stream),
                TypeCode.Guid => s.DeserializeGuid(stream),
                TypeCode.Vector2 => s.DeserializeVector2(stream),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        return new ExtraData(values, s);
    }
    
    public sealed class Schema
    {
        private readonly List<object?> _values = [];
        private bool _isInstanced;
        
        public Key<int> AddIntLateInit() => Add<int>(false);
        public Key<long> AddLongLateInit() => Add<long>(false);
        public Key<double> AddDoubleLateInit() => Add<double>(false);
        public Key<Guid> AddGuidLateInit() => Add<Guid>(false);
        public Key<Vector2> AddVector2LateInit() => Add<Vector2>(false);
        
        public Key<int> AddIntWithDefault(int defaultValue = 0) => Add(false, defaultValue);
        public Key<long> AddLongWithDefault(long defaultValue = 0) => Add(false, defaultValue);
        public Key<double> AddDoubleWithDefault(double defaultValue = 0) => Add(false, defaultValue);
        public Key<Guid> AddGuidWithDefault(Guid defaultValue = default) => Add(false, defaultValue);
        public Key<Vector2> AddVector2WithDefault(Vector2 defaultValue = default) => Add(false, defaultValue);
        
        public Key<int?> AddNullableInt() => Add<int?>(true, null);
        public Key<long?> AddNullableLong() => Add<long?>(true, null);
        public Key<double?> AddNullableDouble() => Add<double?>(true, null);
        public Key<Guid?> AddNullableGuid() => Add<Guid?>(true, null);
        public Key<Vector2?> AddNullableVector2() => Add<Vector2?>(true, null);
        
        public Key<int?> AddNullableIntWithDefault(int defaultValue) => Add<int?>(true, defaultValue);
        public Key<long?> AddNullableLongWithDefault(long defaultValue) => Add<long?>(true, defaultValue);
        public Key<double?> AddNullableDoubleWithDefault(double defaultValue) => Add<double?>(true, defaultValue);
        public Key<Guid?> AddNullableGuidWithDefault(Guid defaultValue) => Add<Guid?>(true, defaultValue);
        public Key<Vector2?> AddNullableVector2WithDefault(Vector2 defaultValue) => Add<Vector2?>(true, defaultValue);
        
        public ExtraData Instance()
        {
            _isInstanced = true;
            return new ExtraData(_values.ToArray(), new SerializationHelper(TypeLocator.Empty));
        }

        private Key<T> Add<T>(bool isNullable)
        {
            ThrowIfIsInstanced();
            _values.Add(null);
            return new Key<T>(_values.Count - 1, isNullable);
        }
        
        private Key<T> Add<T>(bool isNullable, T defaultValue)
        {
            ThrowIfIsInstanced();
            _values.Add(defaultValue);
            return new Key<T>(_values.Count - 1, isNullable);
        }
        
        private void ThrowIfIsInstanced()
        {
            if (_isInstanced)
                throw new InvalidOperationException("Adding values after instancing is not allowed");
        }
    }
    
    public sealed class Key<T>
    {
        internal int Index { get; }
        internal bool IsNullable { get; }
        
        internal Key(int index, bool isNullable)
        {
            Index = index;
            IsNullable = isNullable;
        }
    }
    
    private enum TypeCode : byte { Null, Int, Long, Double, Guid, Vector2 }
}
