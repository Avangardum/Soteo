using AwesomeAssertions;
using Soteo.Core.Dto;

namespace Soteo.Core.Tests;

public sealed class InventoryTests
{
    [Fact]
    public void AddingStackPutsItInFirstSlot()
    {
        var sut = new Inventory(10);

        ItemStack? remainder = sut.Add(ItemStack.Of<TestItem1>(1));

        remainder.Should().BeNull();
        sut[0].Should().Be(ItemStack.Of<TestItem1>(1));
    }

    [Fact]
    public void AddingDifferentItemStacksPutsThemInFirstSlots()
    {
        var sut = new Inventory(10);

        ItemStack? remainder1 = sut.Add(ItemStack.Of<TestItem1>(1));
        ItemStack? remainder2 = sut.Add(ItemStack.Of<TestItem2>(1));

        remainder1.Should().BeNull();
        remainder2.Should().BeNull();
        sut[0].Should().Be(ItemStack.Of<TestItem1>(1));
        sut[1].Should().Be(ItemStack.Of<TestItem2>(1));
    }

    [Fact]
    public void AddingDifferentItemStackWhenFullFails()
    {
        var sut = new Inventory([ItemStack.Of<TestItem1>(1)]);

        ItemStack? remainder = sut.Add(ItemStack.Of<TestItem2>(1));

        remainder.Should().Be(ItemStack.Of<TestItem2>(1));
        sut[0].Should().Be(ItemStack.Of<TestItem1>(1));
    }

    [Fact]
    public void AddingItemStackThatCanBeFullyMergedWithExistingStacksIsFullyMerged()
    {
        var sut = new Inventory([null, ItemStack.Of<TestItem1>(8), ItemStack.Of<TestItem1>(9), null]);

        ItemStack? remainder = sut.Add(ItemStack.Of<TestItem1>(3));

        remainder.Should().BeNull();
        sut[0].Should().BeNull();
        sut[1].Should().Be(ItemStack.Of<TestItem1>(10));
        sut[2].Should().Be(ItemStack.Of<TestItem1>(10));
        sut[3].Should().BeNull();
    }

    [Fact]
    public void AddingItemStackThatCanBePartiallyMergedWithExistingStacksIsMergedAndRemainderIsAddedToFirstEmptySlot()
    {
        var sut = new Inventory([null, ItemStack.Of<TestItem1>(8), ItemStack.Of<TestItem1>(9), null]);

        ItemStack? remainder = sut.Add(ItemStack.Of<TestItem1>(5));

        remainder.Should().BeNull();
        sut[0].Should().Be(ItemStack.Of<TestItem1>(2));
        sut[1].Should().Be(ItemStack.Of<TestItem1>(10));
        sut[2].Should().Be(ItemStack.Of<TestItem1>(10));
        sut[3].Should().BeNull();
    }

    [Fact]
    public void AddingItemStackWhenSpaceIsInsufficientAddsWhatFitsAndReturnsRemainder()
    {
        var sut = new Inventory([ItemStack.Of<TestItem1>(8)]);

        ItemStack? remainder = sut.Add(ItemStack.Of<TestItem1>(4));

        remainder.Should().Be(ItemStack.Of<TestItem1>(2));
        sut[0].Should().Be(ItemStack.Of<TestItem1>(10));
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
