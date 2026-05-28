using System.Numerics;
using AwesomeAssertions;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Exceptions;

namespace Soteo.Core.Gameplay.Tests;

public sealed class ExtraDataTests
{
    [Theory]
    [InlineData(42, 24)]
    [InlineData(67, 69, 228, 420)]
    public void SerializingThenDeserializingIntsReturnsSameValues(params int[] values)
    {
        var schema = new ExtraData.Schema();
        ExtraData.Key<int>[] keys = values.Select(_ => schema.AddIntLateInit()).ToArray();
        ExtraData sut = schema.Instance();
        
        for (int i = 0; i < values.Length; i++)
            sut.Set(keys[i], values[i]);
        
        var stream = new MemoryStream();
        sut.Serialize(stream);
        stream.Position = 0;
        sut = ExtraData.Deserialize(stream);
        
        for (int i = 0; i < values.Length; i++)
            sut.Get(keys[i]).Should().Be(values[i]);
    }
    
    [Fact]
    public void SerializingThenDeserializingMixedValuesReturnsSameValues()
    {
        var schema = new ExtraData.Schema();
        ExtraData.Key<int> intKey = schema.AddIntLateInit();
        ExtraData.Key<long> longKey = schema.AddLongLateInit();
        ExtraData.Key<double> doubleKey = schema.AddDoubleLateInit();
        ExtraData.Key<Guid> guidKey = schema.AddGuidLateInit();
        ExtraData.Key<Vector2> vector2Key = schema.AddVector2LateInit();
        ExtraData.Key<Vector2?> nullableVector2Key = schema.AddNullableVector2();
        ExtraData sut = schema.Instance();
        
        sut.Set(intKey, 42);
        sut.Set(longKey, long.MaxValue);
        sut.Set(doubleKey, 1.46);
        sut.Set(guidKey, Guid.Empty);
        sut.Set(vector2Key, new Vector2(3, 8));
        sut.Set(nullableVector2Key, null);
        
        var stream = new MemoryStream();
        sut.Serialize(stream);
        stream.Position = 0;
        sut = ExtraData.Deserialize(stream);
        
        sut.Get(intKey).Should().Be(42);
        sut.Get(longKey).Should().Be(long.MaxValue);
        sut.Get(doubleKey).Should().Be(1.46);
        sut.Get(guidKey).Should().Be(Guid.Empty);
        sut.Get(vector2Key).Should().Be(new Vector2(3, 8));
        sut.Get(nullableVector2Key).Should().BeNull();
    }
    
    [Fact]
    public void GetWithoutSetThrowsWhenUsingLateInit()
    {
        var schema = new ExtraData.Schema();
        ExtraData.Key<int> intKey = schema.AddIntLateInit();
        ExtraData.Key<long> longKey = schema.AddLongLateInit();
        ExtraData.Key<double> doubleKey = schema.AddDoubleLateInit();
        ExtraData.Key<Guid> guidKey = schema.AddGuidLateInit();
        ExtraData.Key<Vector2> vector2Key = schema.AddVector2LateInit();
        ExtraData sut = schema.Instance();
        
        sut.Invoking(it => it.Get(intKey)).Should().Throw<InvalidOperationException>();
        sut.Invoking(it => it.Get(longKey)).Should().Throw<InvalidOperationException>();
        sut.Invoking(it => it.Get(doubleKey)).Should().Throw<InvalidOperationException>();
        sut.Invoking(it => it.Get(guidKey)).Should().Throw<InvalidOperationException>();
        sut.Invoking(it => it.Get(vector2Key)).Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void GetWithoutSetReturnsSpecifiedDefault()
    {
        var schema = new ExtraData.Schema();
        ExtraData.Key<int> intKey = schema.AddIntWithDefault(42);
        ExtraData.Key<int?> nullableIntKey = schema.AddNullableIntWithDefault(-42);
        ExtraData.Key<long> longKey = schema.AddLongWithDefault(long.MaxValue);
        ExtraData.Key<long?> nullableLongKey = schema.AddNullableLongWithDefault(long.MinValue);
        ExtraData.Key<double> doubleKey = schema.AddDoubleWithDefault(12.34);
        ExtraData.Key<double?> nullableDoubleKey = schema.AddNullableDoubleWithDefault(-12.34);
        var guid1 = Guid.NewGuid();
        ExtraData.Key<Guid> guidKey = schema.AddGuidWithDefault(guid1);
        var guid2 = Guid.NewGuid();
        ExtraData.Key<Guid?> nullableGuidKey = schema.AddNullableGuidWithDefault(guid2);
        ExtraData.Key<Vector2> vector2Key = schema.AddVector2WithDefault(new Vector2(1, 2));
        ExtraData.Key<Vector2?> nullableVector2Key = schema.AddNullableVector2WithDefault(new Vector2(2, 1));
        ExtraData sut = schema.Instance();
        
        sut.Get(intKey).Should().Be(42);
        sut.Get(nullableIntKey).Should().Be(-42);
        sut.Get(longKey).Should().Be(long.MaxValue);
        sut.Get(nullableLongKey).Should().Be(long.MinValue);
        sut.Get(doubleKey).Should().Be(12.34);
        sut.Get(nullableDoubleKey).Should().Be(-12.34);
        sut.Get(guidKey).Should().Be(guid1);
        sut.Get(nullableGuidKey).Should().Be(guid2);
        sut.Get(vector2Key).Should().Be(new Vector2(1, 2));
        sut.Get(nullableVector2Key).Should().Be(new Vector2(2, 1));
    }
    
    [Fact]
    public void GetWithoutSetReturnsNullWhenUsingNullable()
    {
        var schema = new ExtraData.Schema();
        ExtraData.Key<int?> intKey = schema.AddNullableInt();
        ExtraData.Key<long?> longKey = schema.AddNullableLong();
        ExtraData.Key<double?> doubleKey = schema.AddNullableDouble();
        ExtraData.Key<Guid?> guidKey = schema.AddNullableGuid();
        ExtraData.Key<Vector2?> vector2Key = schema.AddNullableVector2();
        ExtraData sut = schema.Instance();
        
        sut.Get(intKey).Should().BeNull();
        sut.Get(longKey).Should().BeNull();
        sut.Get(doubleKey).Should().BeNull();
        sut.Get(guidKey).Should().BeNull();
        sut.Get(vector2Key).Should().BeNull();
    }
    
    [Theory]
    [InlineData(-2293)]
    [InlineData(1000)]
    public void DeserializeWithInvalidCountThrows(int count)
    {
        var schema = new ExtraData.Schema();
        ExtraData.Key<int?> key = schema.AddNullableInt();
        ExtraData sut = schema.Instance();
        
        var stream = new MemoryStream();
        sut.Serialize(stream);
        stream.Position = 0;
        SerializationHelper.SerializeInt(count, stream);
        stream.Position = 0;
        
        FluentActions.Invoking(() => ExtraData.Deserialize(stream)).Should().Throw<BadPacketException>();
    }
    
    [Fact]
    public void OverridingDefaultWithNullThenGettingTheValueReturnsNull()
    {
        var schema = new ExtraData.Schema();
        ExtraData.Key<int?> key = schema.AddNullableIntWithDefault(42);
        ExtraData sut = schema.Instance();
        
        sut.Set(key, null);
        
        sut.Get(key).Should().BeNull();
    }
    
    [Fact]
    public void AddingValuesAfterInstanceThrows()
    {
        var schema = new ExtraData.Schema();
        schema.Instance();
        
        schema.Invoking(it => it.AddIntLateInit()).Should().Throw<InvalidOperationException>();
        schema.Invoking(it => it.AddLongWithDefault()).Should().Throw<InvalidOperationException>();
        schema.Invoking(it => it.AddNullableGuid()).Should().Throw<InvalidOperationException>();
        schema.Invoking(it => it.AddNullableVector2WithDefault(default)).Should().Throw<InvalidOperationException>();
    }
}
