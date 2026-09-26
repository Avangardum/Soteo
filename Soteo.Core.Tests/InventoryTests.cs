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

    private class Item1 : Item
    {
        public override int StackSize => 10;
    }

    private class Item2 : Item
    {
        public override int StackSize => 10;
    }
}
