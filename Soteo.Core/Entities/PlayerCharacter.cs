using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.Items;

namespace Soteo.Core.Entities;

public sealed class PlayerCharacter : Unit
{
    public PlayerCharacter
    (
        Guid id,
        Guid controllingPlayerId,
        IUnitNode node,
        IEntityManager entityManager,
        IInventory inventory,
        IEquipmentInventory equipmentInventory,
        ICrossInventoryHelper crossInventoryHelper,
        IServiceProvider serviceProvider
    ) : base
    (
        id,
        controllingPlayerId,
        node,
        entityManager,
        inventory,
        equipmentInventory,
        crossInventoryHelper,
        serviceProvider
    )
    {
        SetInnateSpecialAbility<RangedAttackAbility>(AbilitySlot.Attack);
        SetInnateSpecialAbility<RecallAbility>(AbilitySlot.Recall);
        AddInnateHotbarAbility<BloodSacrificeAbility>();
        AddInnateHotbarAbility<HealAbility>();

        Inventory.Put(0, ItemStack.Of<LogItem>(1));
        Inventory.Put(1, ItemStack.Of<LogItem>(1));
        Inventory.Put(2, ItemStack.Of<RockItem>(1));
        Inventory.Put(3, ItemStack.Of<TestBootsItem>(1));
    }

    public static PlayerCharacter FromSnapshot
    (
        UnitSnapshot snapshot,
        IUnitNode node,
        IEntityManager entityManager,
        IInventory inventory,
        IEquipmentInventory equipmentInventory,
        ICrossInventoryHelper crossInventoryHelper,
        IServiceProvider serviceProvider
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
            serviceProvider
        );
    }
}
