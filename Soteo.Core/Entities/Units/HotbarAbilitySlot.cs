namespace Soteo.Core.Entities.Units;

public sealed class HotbarAbilitySlot : AbilitySlot
{
    private static readonly Dictionary<int, HotbarAbilitySlot> Instances = [];

    public int Index { get; }

    private HotbarAbilitySlot(int index)
    {
        Index = index;
    }

    internal static HotbarAbilitySlot Instance(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(null, "Index should not be negative");
        HotbarAbilitySlot? value;
        if (Instances.TryGetValue(index, out value))
        {
            return value;
        }
        else
        {
            value = new HotbarAbilitySlot(index);
            Instances[index] = value;
            return value;
        }
    }

    public override string ToString() => "Hotbar" + Index;

    public static HotbarAbilitySlot operator++(HotbarAbilitySlot operand) => Instance(operand.Index + 1);
    public static HotbarAbilitySlot operator--(HotbarAbilitySlot operand) => Instance(operand.Index - 1);
}
