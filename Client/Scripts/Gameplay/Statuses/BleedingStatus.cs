using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Enums;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Statuses;

public sealed class BleedingStatus : Status
{
    public const float Time = 5;
    public const float TickInterval = 0.2f;
    
    public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Refresh;

    public override IReadOnlyList<StatModifier> StatModifiers(StatusContext context) =>
    [
        new(Stat.MoveSpeed, StatModifierKind.Add, -20)
    ];

    public override void Tick(StatusContext context)
    {
        const double damagePerSecond = 5;
        double damage = damagePerSecond * context.TickInterval;
        context.Unit.TakeDamage(damage, context);
    }
}