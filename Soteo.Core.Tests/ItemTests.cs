using AwesomeAssertions;
using Soteo.Core.Items;

namespace Soteo.Core.Tests;

public sealed class ItemTests
{
    [Fact]
    public void InstanceReturnsSameInstanceForEveryCall()
    {
        var instance1 = Item.Instance<TestItem>();
        var instance2 = Item.Instance<TestItem>();
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void InstancingStatusWithNewThrows()
    {
        FluentActions.Invoking(() => new TestItem()).Should().Throw<InvalidOperationException>();
    }

    private class TestItem : Item;
}
