using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Util;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Abilities;

public abstract class Attack : Ability
{
    public override CanTarget Targeting => CanTarget.Enemy | CanTarget.Character | CanTarget.Building;
    public override Scalable<float> StaticRange => 0;
    public override Scalable<float> StaticUseTime => 0;
    public override Scalable<float> StaticCooldown => 0;

    protected override float DynamicUseTime(AbilityContext context) =>
        AttackInterval(context) * context.User.Stats[Stat.AttackUseTimeFraction];
    
    protected override float DynamicCooldown(AbilityContext context) =>
        AttackInterval(context) * (1 - context.User.Stats[Stat.AttackUseTimeFraction]);

    private float AttackInterval(AbilityContext context) => 1 / (context.User.Stats[Stat.AttackSpeed] / 1000);

    protected override float DynamicRange(AbilityContext context) => context.User.Stats[Stat.AttackRange];
}