namespace Soteo.Core.Items;

public sealed class TestBootsItem : Item
{
    public override EquipmentSlot? EquippableTo => EquipmentSlot.Feet;

    public override IReadOnlyList<Ability> HotbarAbilities =>
    [
        Ability.Instance<ProjectileBurstAbility>(),
        Ability.Instance<VampireAbility>()
    ];
}
