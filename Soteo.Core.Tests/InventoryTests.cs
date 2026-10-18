using AwesomeAssertions;
using Soteo.Core.Dto;

namespace Soteo.Core.Tests;

public sealed class InventoryTests
{
    [Fact]
    public void AddingStackPutsItInFirstSlot()
    {
        var sut = new Inventory(10);

        sut.TryAdd(ItemStack.Of<TestItem1>(1)).Should().BeTrue();

        sut[0].Should().Be(ItemStack.Of<TestItem1>(1));
    }

    [Fact]
    public void AddingDifferentItemStacksPutsThemInFirstSlots()
    {
        var sut = new Inventory(10);

        sut.TryAdd(ItemStack.Of<TestItem1>(1)).Should().BeTrue();
        sut.TryAdd(ItemStack.Of<TestItem2>(1)).Should().BeTrue();

        sut[0].Should().Be(ItemStack.Of<TestItem1>(1));
        sut[1].Should().Be(ItemStack.Of<TestItem2>(1));
    }

    [Fact]
    public void AddingDifferentItemStackWhenFullFails()
    {
        var sut = new Inventory([ItemStack.Of<TestItem1>(1)]);

        sut.TryAdd(ItemStack.Of<TestItem2>(1)).Should().BeFalse();

        sut[0].Should().Be(ItemStack.Of<TestItem1>(1));
    }

    [Fact]
    public void AddingItemStackThatCanBeFullyMergedWithExistingStacksIsFullyMerged()
    {
        var sut = new Inventory([null, ItemStack.Of<TestItem1>(8), ItemStack.Of<TestItem1>(9), null]);

        sut.TryAdd(ItemStack.Of<TestItem1>(3)).Should().BeTrue();

        sut[0].Should().Be(null);
        sut[1].Should().Be(ItemStack.Of<TestItem1>(10));
        sut[2].Should().Be(ItemStack.Of<TestItem1>(10));
        sut[3].Should().Be(null);
    }

    [Fact]
    public void AddingItemStackThatCanBePartiallyMergedWithExistingStacksIsMergedAndRemainderIsAddedToFirstEmptySlot()
    {
        var sut = new Inventory([null, ItemStack.Of<TestItem1>(8), ItemStack.Of<TestItem1>(9), null]);

        sut.TryAdd(ItemStack.Of<TestItem1>(5)).Should().BeTrue();

        sut[0].Should().Be(ItemStack.Of<TestItem1>(2));
        sut[1].Should().Be(ItemStack.Of<TestItem1>(10));
        sut[2].Should().Be(ItemStack.Of<TestItem1>(10));
        sut[3].Should().Be(null);
    }

    [Fact]
    public void PutAtSwapsStacksOfDifferentItems()
    {
        var sut = new Inventory([ItemStack.Of<TestItem1>(1), null]);

        ItemStack? result = sut.PutAt(0, ItemStack.Of<TestItem2>(1));

        sut[0].Should().Be(ItemStack.Of<TestItem2>(1));
        sut[1].Should().BeNull();
        result.Should().Be(ItemStack.Of<TestItem1>(1));
    }

    [Fact]
    public void PutAtSwapsStacksOfSameItemsIfExistingStackIsFull()
    {
        var sut = new Inventory([ItemStack.Of<TestItem1>(10), null]);

        ItemStack? result = sut.PutAt(0, ItemStack.Of<TestItem1>(1));

        sut[0].Should().Be(ItemStack.Of<TestItem1>(1));
        sut[1].Should().BeNull();
        result.Should().Be(ItemStack.Of<TestItem1>(10));
    }

    [Fact]
    public void PutAtFullyMergesStacksIfPossible()
    {
        var sut = new Inventory([ItemStack.Of<TestItem1>(1), null]);

        ItemStack? result = sut.PutAt(0, ItemStack.Of<TestItem1>(1));

        sut[0].Should().Be(ItemStack.Of<TestItem1>(2));
        sut[1].Should().BeNull();
        result.Should().BeNull();
    }

    [Fact]
    public void PutAtPartiallyMergesStacksOfSameItemAndReturnsRemainderIfFullMergeIsImpossible()
    {
        var sut = new Inventory([ItemStack.Of<TestItem1>(6), null]);

        ItemStack? result = sut.PutAt(0, ItemStack.Of<TestItem1>(6));

        sut[0].Should().Be(ItemStack.Of<TestItem1>(10));
        sut[1].Should().BeNull();
        result.Should().Be(ItemStack.Of<TestItem1>(2));
    }

    [Fact]
    public void RemoveAtFilledSlotRemovesAndReturnsStack()
    {
        var sut = new Inventory([ItemStack.Of<TestItem1>(2)]);

        ItemStack? result = sut.RemoveAt(0);

        sut[0].Should().BeNull();
        result.Should().Be(ItemStack.Of<TestItem1>(2));
    }

    [Fact]
    public void RemoveAtEmptySlotLeavesItEmptyAndReturnsNull()
    {
        var sut = new Inventory([null]);

        ItemStack? result = sut.RemoveAt(0);

        sut[0].Should().BeNull();
        result.Should().BeNull();
    }
}
