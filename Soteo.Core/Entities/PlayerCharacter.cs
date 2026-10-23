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
        IServiceProvider serviceProvider
    ) : base(id, controllingPlayerId, node, entityManager, serviceProvider)
    {
        SetAbility<BloodSacrificeAbility>(AbilitySlot.Class0, 1);
        SetAbility<HealAbility>(AbilitySlot.Class1, 1);
        SetAbility<ProjectileBurstAbility>(AbilitySlot.Class2, 1);
        SetAbility<VampireAbility>(AbilitySlot.Class3, 1);
        SetAbility<RecallAbility>(AbilitySlot.Recall, 1);
        SetAbility<RangedAttackAbility>(AbilitySlot.Attack, 1);

        Inventory.PutAt(0, ItemStack.Of<LogItem>(1));
        Inventory.PutAt(1, ItemStack.Of<LogItem>(1));
        Inventory.PutAt(2, ItemStack.Of<RockItem>(1));
        Inventory.PutAt(3, ItemStack.Of<BlinkBootsItem>(1));
    }

    /// <inheritdoc/>
    public override IReadOnlyList<EquipmentSlot> EquipmentSlots =>
    [
        EquipmentSlot.MainHand,
        EquipmentSlot.Offhand,
        EquipmentSlot.Head,
        EquipmentSlot.Body,
        EquipmentSlot.Feet,
        EquipmentSlot.Potion
    ];

    public static PlayerCharacter FromSnapshot
    (
        UnitSnapshot snapshot,
        IUnitNode node,
        IEntityManager entityManager,
        IServiceProvider serviceProvider
    )
    {
        return new(snapshot.Id, snapshot.ControllingPlayerId.Required, node, entityManager, serviceProvider);
    }
}
