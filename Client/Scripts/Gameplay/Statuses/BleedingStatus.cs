using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Enums;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Statuses;

public sealed class BleedingStatus : Status
{
    public const float Time = 5;
    public const float TickInterval = 0.2f;
    
    public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.StackAndRefresh;

    public override IReadOnlyList<StatModifier> StatModifiers(StatusContext context) =>
    [
        new(Stat.MoveSpeed, StatModifierKind.Add, -20)
    ];

    public override void Tick(StatusContext context)
    {
        const float damagePerSecond = 5;
        float damage = damagePerSecond * context.TickInterval;
        context.Unit.TakeDamage(damage, context);
    }
}