using Soteo.Gameplay.Enums;

namespace Soteo.Gameplay.Abilities;

public sealed class HealAbility : Ability<HealAbility>
{
    public override int MaxLevel => 4;
    public override Scalable<float> StaticManaCost => [100, 120, 160, 180];
    private Scalable<float> Heal => [200, 300, 400, 500];
    public override Scalable<float> StaticCooldown => [15, 13, 11, 9];
    public override Scalable<float> StaticUseTime => 0.5f;
    public override Scalable<float> StaticRange => 300;
    public override AbilityTargetFlags TargetFlags => AbilityTargetFlags.Unit;

    public override void TakeEffect(AbilityUseContext context)
    {
        base.TakeEffect(context);
        context.TargetUnit!.RestoreHealth(Heal[context.Level], context.User, this);
    }

    public override AbilityValidationResult Validate(AbilityUseContext context, bool strict = true)
    {
        AbilityValidationResult baseValidationResult = base.Validate(context, strict);
        if (baseValidationResult != AbilityValidationResult.Ok) return baseValidationResult;
        
        if (!context.User.IsAlliedTo(context.TargetUnit!)) return AbilityValidationResult.InvalidTarget;
        
        return AbilityValidationResult.Ok;
    }
}