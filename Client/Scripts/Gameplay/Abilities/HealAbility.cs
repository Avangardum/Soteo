using Soteo.Gameplay.Enums;

namespace Soteo.Gameplay.Abilities;

public sealed class HealAbility : Ability
{
    public override int MaxLevel => 4;
    public override Scalable<float> StaticManaCost => [100, 120, 160, 180];
    private Scalable<float> Heal => [200, 300, 400, 500];
    public override Scalable<float> StaticCooldown => [15, 13, 11, 9];
    public override Scalable<float> StaticUseTime => 0.5f;
    public override Scalable<float> StaticRange => 300;
    public override CanTarget Targeting => CanTarget.Ally | CanTarget.Character;

    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.TargetUnit!.RestoreHealth(Heal[context.Level], context.User, this);
    }
}