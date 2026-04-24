using Soteo.Gameplay.Enums;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Abilities;

public abstract class Attack<T> : Ability<T> where T : Ability<T>, new()
{
    public override CanTarget Targeting => CanTarget.Enemy | CanTarget.Character | CanTarget.Building;
    public override Scalable<float> StaticRange => 0;
    public override Scalable<float> StaticUseTime => 0;
    public override Scalable<float> StaticCooldown => 0;

    protected override float DynamicUseTime(AbilityUseContext context) =>
        AttackInterval(context) * context.User.Stats[Stat.AttackUseTimeFraction];
    
    protected override float DynamicCooldown(AbilityUseContext context) =>
        AttackInterval(context) * (1 - context.User.Stats[Stat.AttackUseTimeFraction]);

    private float AttackInterval(AbilityUseContext context) => 1 / (context.User.Stats[Stat.AttackSpeed] / 1000);

    protected override float DynamicRange(AbilityUseContext context) => context.User.Stats[Stat.AttackRange];
}