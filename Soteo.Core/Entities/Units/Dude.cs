using Soteo.Core.Commands;

namespace Soteo.Core.Entities.Units;

public sealed class Dude : Unit
{
    public Dude
    (
        Guid id,
        Guid? controllingPlayerId,
        IUnitNode node,
        Inventory inventory,
        EquipmentInventory equipmentInventory,
        Services services
    ) : base
    (
        id,
        controllingPlayerId,
        node,
        inventory,
        equipmentInventory,
        services,
        new Ai()
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

    public static Dude FromSnapshot
    (
        UnitSnapshot snapshot,
        IUnitNode node,
        Inventory inventory,
        EquipmentInventory equipmentInventory,
        Services services
    )
    {
        return new
        (
            snapshot.Id,
            controllingPlayerId: null,
            node,
            inventory,
            equipmentInventory,
            services
        );
    }

    private sealed class Ai : UnitAi
    {
        public override void Think(UnitAiContext context)
        {
            context.SetCommand(new MoveCommand(Vector2.Zero));
        }
    }
}
