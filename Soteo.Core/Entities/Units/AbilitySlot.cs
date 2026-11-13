namespace Soteo.Core.Entities.Units;

public closed class AbilitySlot
{
    private static readonly BidirectionalDictionary<SpecialAbilitySlot, int> SpecialIndices = [];

    public static readonly SpecialAbilitySlot Attack = new(nameof(Attack));
    public static readonly SpecialAbilitySlot Recall = new(nameof(Recall));

    static AbilitySlot()
    {
        SpecialIndices[Attack] = -1;
        SpecialIndices[Recall] = -2;

        AllSpecial = SpecialIndices.Keys.ToList();
    }

    public static HotbarAbilitySlot Hotbar(int index) => HotbarAbilitySlot.Instance(index);

    public static IReadOnlyList<SpecialAbilitySlot> AllSpecial { get; }

    internal int ToInt() =>
        this is HotbarAbilitySlot h ? h.Index : SpecialIndices[(SpecialAbilitySlot)this];

    internal static AbilitySlot FromInt(int index) =>
        index >= 0 ? Hotbar(index) : SpecialIndices.Inverse[index];

    public override string ToString() => nameof(AbilitySlot);
}
