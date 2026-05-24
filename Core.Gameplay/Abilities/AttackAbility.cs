using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Enums;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Abilities;

public abstract class AttackAbility : Ability
{
    public override CanTarget Targeting => CanTarget.Enemy | CanTarget.Character | CanTarget.Building;
    public override Scalable<double> StaticRange => 0;
    public override Scalable<double> StaticUseTime => 0;
    public override Scalable<double> StaticCooldown => 0;

    protected override double DynamicUseTime(AbilityContext context) =>
        AttackInterval(context) * context.User.Stats[Stat.AttackUseTimeFraction];
    
    protected override double DynamicCooldown(AbilityContext context) =>
        AttackInterval(context) * (1 - context.User.Stats[Stat.AttackUseTimeFraction]);

    private double AttackInterval(AbilityContext context) =>
        1 / (context.User.Stats[Stat.AttackSpeed] / 1000);

    protected override double DynamicRange(AbilityContext context) => context.User.Stats[Stat.AttackRange];
}