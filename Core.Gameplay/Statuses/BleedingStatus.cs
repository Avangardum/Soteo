using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Enums;

namespace Soteo.Core.Gameplay.Statuses;

public sealed class BleedingStatus : Status
{
    public const double Time = 5;
    public const double TickInterval = 0.2;
    
    public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Refresh;

    public override IReadOnlyList<StatModifier> StatModifiers(StatusContext context) =>
    [
        new(Stat.MoveSpeed, StatModifierKind.Add, -20)
    ];

    public override void Tick(StatusContext context, double delta)
    {
        const double damagePerSecond = 5;
        double damage = damagePerSecond * delta;
        context.Unit.TakeDamage(damage, context);
    }
}
