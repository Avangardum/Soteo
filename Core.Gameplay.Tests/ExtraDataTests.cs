using System.Numerics;
using AwesomeAssertions;
using Soteo.Core.Gameplay.Dto;

namespace Soteo.Core.Gameplay.Tests;

public sealed class ExtraDataTests
{
    [Theory]
    [InlineData(42, 24)]
    [InlineData(67, 69, 228, 420)]
    public void GetSetInt(params int[] values)
    {
        var builder = new ExtraData.Builder();
        ExtraData.Key<int>[] keys = values.Select(_ => builder.AddIntLateInit()).ToArray();
        ExtraData sut = builder.Build();
        
        for (int i = 0; i < values.Length; i++)
            sut.Set(keys[i], values[i]);
        
        for (int i = 0; i < values.Length; i++)
        {
            int returned = sut.Get(keys[i]);
            returned.Should().Be(values[i]);
        }
    }
    
    [Fact]
    public void GetSetMixed()
    {
        const int intValue = 42;
        const long longValue = long.MaxValue;
        const double doubleValue = 1.46;
        var guidValue = Guid.NewGuid();
        var vector2Value = new Vector2(3, 8);
        
        var builder = new ExtraData.Builder();
        ExtraData.Key<int> intKey = builder.AddIntLateInit();
        ExtraData.Key<long> longKey = builder.AddLongLateInit();
        ExtraData.Key<double> doubleKey = builder.AddDoubleLateInit();
        ExtraData.Key<Guid> guidKey = builder.AddGuidLateInit();
        ExtraData.Key<Vector2> vector2Key = builder.AddVector2LateInit();
        ExtraData.Key<Vector2?> nullableVector2Key = builder.AddNullableVector2();
        ExtraData sut = builder.Build();
        
        sut.Set(intKey, intValue);
        sut.Set(longKey, longValue);
        sut.Set(doubleKey, doubleValue);
        sut.Set(guidKey, guidValue);
        sut.Set(vector2Key, vector2Value);
        sut.Set(nullableVector2Key, null);
        
        sut.Get(intKey).Should().Be(intValue);
        sut.Get(longKey).Should().Be(longValue);
        sut.Get(doubleKey).Should().Be(doubleValue);
        sut.Get(guidKey).Should().Be(guidValue);
        sut.Get(vector2Key).Should().Be(vector2Value);
        sut.Get(nullableVector2Key).Should().BeNull();
    }
    
    [Fact]
    public void GetWithoutSetThrowsWhenUsingLateInit()
    {
        var builder = new ExtraData.Builder();
        ExtraData.Key<int> intKey = builder.AddIntLateInit();
        ExtraData.Key<long> longKey = builder.AddLongLateInit();
        ExtraData.Key<double> doubleKey = builder.AddDoubleLateInit();
        ExtraData.Key<Guid> guidKey = builder.AddGuidLateInit();
        ExtraData.Key<Vector2> vector2Key = builder.AddVector2LateInit();
        ExtraData sut = builder.Build();
        
        sut.Invoking(it => it.Get(intKey)).Should().Throw<InvalidOperationException>();
        sut.Invoking(it => it.Get(longKey)).Should().Throw<InvalidOperationException>();
        sut.Invoking(it => it.Get(doubleKey)).Should().Throw<InvalidOperationException>();
        sut.Invoking(it => it.Get(guidKey)).Should().Throw<InvalidOperationException>();
        sut.Invoking(it => it.Get(vector2Key)).Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void GetWithoutSetReturnsSpecifiedDefault()
    {
        var builder = new ExtraData.Builder();
        ExtraData.Key<int> intKey = builder.AddIntWithDefault(42);
        ExtraData.Key<int?> nullableIntKey = builder.AddNullableIntWithDefault(-42);
        ExtraData.Key<long> longKey = builder.AddLongWithDefault(long.MaxValue);
        ExtraData.Key<long?> nullableLongKey = builder.AddNullableLongWithDefault(long.MinValue);
        ExtraData.Key<double> doubleKey = builder.AddDoubleWithDefault(12.34);
        ExtraData.Key<double?> nullableDoubleKey = builder.AddNullableDoubleWithDefault(-12.34);
        var guid1 = Guid.NewGuid();
        ExtraData.Key<Guid> guidKey = builder.AddGuidWithDefault(guid1);
        var guid2 = Guid.NewGuid();
        ExtraData.Key<Guid?> nullableGuidKey = builder.AddNullableGuidWithDefault(guid2);
        ExtraData.Key<Vector2> vector2Key = builder.AddVector2WithDefault(new Vector2(1, 2));
        ExtraData.Key<Vector2?> nullableVector2Key = builder.AddNullableVector2WithDefault(new Vector2(2, 1));
        ExtraData sut = builder.Build();
        
        sut.Get(intKey).Should().Be(42);
        sut.Get(nullableIntKey).Should().Be(-42);
        sut.Get(longKey).Should().Be(long.MaxValue);
        sut.Get(nullableLongKey).Should().Be(long.MinValue);
        sut.Get(doubleKey).Should().Be(12.34);
        sut.Get(nullableDoubleKey).Should().Be(-12.34);
        sut.Get(guidKey).Should().Be(guid1);
        sut.Get(nullableGuidKey).Should().Be(guid2);
    }
    
    [Fact]
    public void GetWithoutSetReturnsNullWhenUsingNullable()
    {
        var builder = new ExtraData.Builder();
        ExtraData.Key<int?> intKey = builder.AddNullableInt();
        ExtraData.Key<long?> longKey = builder.AddNullableLong();
        ExtraData.Key<double?> doubleKey = builder.AddNullableDouble();
        ExtraData.Key<Guid?> guidKey = builder.AddNullableGuid();
        ExtraData.Key<Vector2?> vector2Key = builder.AddNullableVector2();
        ExtraData sut = builder.Build();
        
        sut.Get(intKey).Should().BeNull();
        sut.Get(longKey).Should().BeNull();
        sut.Get(doubleKey).Should().BeNull();
        sut.Get(guidKey).Should().BeNull();
        sut.Get(vector2Key).Should().BeNull();
    }
    
    // todo reconsider default switch case warning
}
