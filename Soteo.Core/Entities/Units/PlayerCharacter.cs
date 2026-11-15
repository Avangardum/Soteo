namespace Soteo.Core.Entities.Units;

public sealed class PlayerCharacter : Unit
{
    public PlayerCharacter
    (
        Guid id,
        Guid controllingPlayerId,
        IUnitNode node,
        IEntityManager entityManager,
        Inventory inventory,
        EquipmentInventory equipmentInventory,
        ICrossInventoryHelper crossInventoryHelper,
        Services services
    ) : base
    (
        id,
        controllingPlayerId,
        node,
        entityManager,
        inventory,
        equipmentInventory,
        crossInventoryHelper,
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

    public static PlayerCharacter FromSnapshot
    (
        UnitSnapshot snapshot,
        IUnitNode node,
        IEntityManager entityManager,
        Inventory inventory,
        EquipmentInventory equipmentInventory,
        ICrossInventoryHelper crossInventoryHelper,
        Services services
    )
    {
        return new
        (
            snapshot.Id,
            snapshot.ControllingPlayerId.Required,
            node,
            entityManager,
            inventory,
            equipmentInventory,
            crossInventoryHelper,
            services
        );
    }
}
