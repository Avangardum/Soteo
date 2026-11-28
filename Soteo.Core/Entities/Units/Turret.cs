using Soteo.Core.Commands;
using Soteo.Core.Snapshots;

namespace Soteo.Core.Entities.Units;

public sealed class Turret : Unit
{
    public Turret
    (
        Guid id,
        IUnitNode node,
        Services services
    ) : base(id, node, new Inventory(0), new EquipmentInventory(), services, new Ai())
    {
        SetInnateSpecialAbility<RangedAttackAbility>(AbilitySlot.Attack);
    }

    private sealed class Ai : UnitAi
    {
        public override void Think(UnitAiContext context)
        {
            UnitSnapshot? closestEnemy =
                context.Snapshot.Units
                    .NotInFaction(context.Self.Faction)
                    .InCircle(context.Self.Position, context.Self.Stats[Stat.AttackRange])
                    .ClosestTo(context.Self.Position);
            if (closestEnemy == null) return;
            context.SetCommand(new UseAbilityCommand
            {
                Slot = AbilitySlot.Attack,
                Repeat = true,
                Target = new AbilityTargetSnapshot { UnitId = closestEnemy.Id }
            });
        }
    }
}
