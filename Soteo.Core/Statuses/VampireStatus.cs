using Soteo.Core.Abilities;
using Soteo.Core.Entities;
using Soteo.Util;

namespace Soteo.Core.Statuses;

public sealed class VampireStatus : AbilityPassiveStatus
{
    public override IReadOnlyList<StatModifier> StatModifiers(StatusContext context) =>
    [
        new(Stat.HealthRegen, StatModifierKind.Set, 0)
    ];

    public override void Tick(StatusContext context, double delta)
    {
        double maxStableHealth = context.Unit.Stats[Stat.MaxHealth] * 0.7;
        double unstableHealth = context.Unit.Stats[Stat.CurrentHealth] - maxStableHealth;
        if (unstableHealth > 0)
        {
            const double healthDrainPerSecond = 5;
            double healthDrain = Maths.Min(unstableHealth, healthDrainPerSecond * delta);
            context.Unit.SpendHealth(healthDrain, context.SourceAbilityContext.Required.Ability);
        }
    }

    public override void OnDealAttackDamage(StatusContext context, Unit target, double damage)
    {
        double lifestealFactor =
            context.SourceAbilityAs<VampireAbility>().LifestealFactor;
        context.Unit.RestoreHealth(damage * lifestealFactor, context);
        target.AddStatus<BleedingStatus>(BleedingStatus.Time, context);
    }
}
