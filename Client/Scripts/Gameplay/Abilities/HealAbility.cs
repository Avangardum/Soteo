using Soteo.Gameplay.Enums;

namespace Soteo.Gameplay.Abilities;

public sealed class HealAbility : Ability<HealAbility>
{
    public override int MaxLevel => 4;
    public override Scalable<float> ManaCost => [100, 120, 160, 180];
    private Scalable<float> Heal => [200, 300, 400, 500];
    public override Scalable<float> Cooldown => [15, 13, 11, 9];
    public override Scalable<float> UseTimeSec => 0.5f;
    public override Scalable<float> Range => 300;
    public override AbilityTargetFlags TargetFlags => AbilityTargetFlags.Unit;

    public override void TakeEffect(AbilityUseContext context)
    {
        base.TakeEffect(context);
        context.TargetUnit!.Heal(Heal[context.Level], context.Caster, this);
    }

    public override AbilityValidationResult Validate(AbilityUseContext context, bool strict = true)
    {
        AbilityValidationResult baseValidationResult = base.Validate(context, strict);
        if (baseValidationResult != AbilityValidationResult.Ok) return baseValidationResult;
        
        if (!context.Caster.IsAlliedTo(context.TargetUnit!)) return AbilityValidationResult.InvalidTarget;
        
        return AbilityValidationResult.Ok;
    }
}