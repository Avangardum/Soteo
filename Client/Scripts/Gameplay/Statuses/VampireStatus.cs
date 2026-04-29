using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Enums;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Statuses;

public sealed class VampireStatus : Status
{
    public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Replace;

    public override IReadOnlyList<StatModifier> StatModifiers(StatusContext context) =>
    [
        new(Stat.HealthRegen, StatModifierKind.Set, 0)
    ];

    public override void Tick(StatusContext context)
    {
        float maxStableHealth = context.Unit.Stats[Stat.MaxHealth] * 0.7f;
        float unstableHealth = context.Unit.Stats[Stat.CurrentHealth] - maxStableHealth;
        if (unstableHealth > 0)
        {
            const float healthDrainPerSecond = 5;
            float healthDrain = Mathf.Min(unstableHealth, healthDrainPerSecond * context.TickInterval);
            context.Unit.SpendHealth(healthDrain, context.AbilityContext.Required.Ability);
        }
    }

    public override void OnDealAttackDamage(StatusContext context, Unit target, float damage)
    {
        float lifestealFactor = context.AbilityAs<VampireAbility>().LifestealFactor[context.AbilityContext.Level];
        context.Unit.RestoreHealth(damage * lifestealFactor, context.Unit, context.AbilityContext.Ability);
    }
}