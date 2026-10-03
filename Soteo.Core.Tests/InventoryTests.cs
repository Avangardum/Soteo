using AwesomeAssertions;
using Soteo.Core.Dto;
using Soteo.Core.Items;

namespace Soteo.Core.Tests;

public sealed class InventoryTests
{
    [Fact]
    public void AddingStackPutsItInFirstSlot()
    {
        var sut = new Inventory(10);

        sut.TryAdd(ItemStack.Of<Item1>(1)).Should().BeTrue();

        sut[0].Should().Be(ItemStack.Of<Item1>(1));
    }

    [Fact]
    public void AddingDifferentItemStacksPutsThemInFirstSlots()
    {
        var sut = new Inventory(10);

        sut.TryAdd(ItemStack.Of<Item1>(1)).Should().BeTrue();
        sut.TryAdd(ItemStack.Of<Item2>(1)).Should().BeTrue();

        sut[0].Should().Be(ItemStack.Of<Item1>(1));
        sut[1].Should().Be(ItemStack.Of<Item2>(1));
    }

    [Fact]
    public void AddingDifferentItemStackWhenFullFails()
    {
        var sut = new Inventory([ItemStack.Of<Item1>(1)]);

        sut.TryAdd(ItemStack.Of<Item2>(1)).Should().BeFalse();

        sut[0].Should().Be(ItemStack.Of<Item1>(1));
    }

    [Fact]
    public void AddingItemStackThatCanBeFullyMergedWithExistingStacksIsFullyMerged()
    {
        var sut = new Inventory([null, ItemStack.Of<Item1>(8), ItemStack.Of<Item1>(9), null]);

        sut.TryAdd(ItemStack.Of<Item1>(3)).Should().BeTrue();

        sut[0].Should().Be(null);
        sut[1].Should().Be(ItemStack.Of<Item1>(10));
        sut[2].Should().Be(ItemStack.Of<Item1>(10));
        sut[3].Should().Be(null);
    }

    [Fact]
    public void AddingItemStackThatCanBePartiallyMergedWithExistingStacksIsMergedAndRemainderIsAddedToFirstEmptySlot()
    {
        var sut = new Inventory([null, ItemStack.Of<Item1>(8), ItemStack.Of<Item1>(9), null]);

        sut.TryAdd(ItemStack.Of<Item1>(5)).Should().BeTrue();

        sut[0].Should().Be(ItemStack.Of<Item1>(2));
        sut[1].Should().Be(ItemStack.Of<Item1>(10));
        sut[2].Should().Be(ItemStack.Of<Item1>(10));
        sut[3].Should().Be(null);
    }

    private class Item1 : Item
    {
        public override long MaxStackCount => 10;
    }

    private class Item2 : Item
    {
        public override long MaxStackCount => 10;
    }
}
