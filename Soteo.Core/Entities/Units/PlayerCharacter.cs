namespace Soteo.Core.Entities.Units;

public sealed class PlayerCharacter : Unit
{
    public PlayerCharacter
    (
        Guid id,
        Guid controllingPlayerId,
        IUnitNode node,
        Services services
    ) : base
    (
        id,
        controllingPlayerId,
        node,
        new Inventory(8),
        new EquipmentInventory(Enum.GetValues<EquipmentSlot>()),
        services
    )
    {
        SetInnateSpecialAbility<RangedAttackAbility>(AbilitySlot.Attack);
        SetInnateSpecialAbility<RecallAbility>(AbilitySlot.Recall);
        AddInnateHotbarAbility<BloodSacrificeAbility>();
        AddInnateHotbarAbility<HealAbility>();

        Inventory.Put<LogItem>(0, 1);
        Inventory.Put<LogItem>(1, 1);
        Inventory.Put<RockItem>(2, 1);
        Inventory.Put<TestBootsItem>(3, 1);
    }
}
