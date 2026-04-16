using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public sealed class HealAbility : UnitTargetedAbility<HealAbility>
{
    public override int MaxLevel => 4;

    public override Scalable<float> ManaCost => [100, 120, 160, 180];
    
    private Scalable<float> Heal => [200, 300, 400, 500];

    public override Scalable<float> Cooldown => [15, 13, 11, 9];

    public override Scalable<float> CastTimeSeconds => 0.5f;

    public override Scalable<float> CastRange => 300;

    public override bool IsValidTarget(AbilityCastContext context, Unit target) => target.IsAlliedTo(context.Caster);

    public override void Cast(AbilityCastContext context, Unit target) =>
        target.Heal(Heal[context.Level], context.Caster, this);
}