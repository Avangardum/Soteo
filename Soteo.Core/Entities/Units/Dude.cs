using Soteo.Core.Commands;
using Soteo.Core.Snapshots;

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
        AddInnateHotbarAbility<VampireAbility>();
    }

    private sealed class Ai : UnitAi
    {
        public override void Think(UnitAiContext context)
        {
            UnitSnapshot? closestEnemy =
                context.Snapshot.Units.NotInFaction(context.Self.Faction).ClosestTo(context.Self.Position);
            if (closestEnemy == null) return;
            context.SetCommand(new UseAbilityCommand
            {
                Slot = AbilitySlot.Attack,
                TargetUnitId = closestEnemy.Id
            });
        }
    }
}
