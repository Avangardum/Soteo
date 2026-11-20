using Soteo.Core.Commands;

namespace Soteo.Core.Entities.Units;

public sealed class Dude : Unit
{
    public Dude
    (
        Guid id,
        IUnitNode node,
        Services services
    ) : base
    (
        id,
        node,
        new Inventory(0),
        new EquipmentInventory(),
        services,
        new Ai()
    )
    {
        SetInnateSpecialAbility<RangedAttackAbility>(AbilitySlot.Attack);
    }

    private sealed class Ai : UnitAi
    {
        public override void Think(UnitAiContext context)
        {
            context.SetCommand(new MoveCommand(Vector2.Zero));
        }
    }
}
