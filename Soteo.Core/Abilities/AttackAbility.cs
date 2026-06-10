using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Enums;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Abilities;

public abstract class AttackAbility : Ability
{
    public override CanTarget Targeting => CanTarget.Enemy | CanTarget.Character | CanTarget.Building;
    public sealed override Scalable<double> StaticRange => 0;
    public sealed override Scalable<double> StaticUseTime => 0;
    public sealed override Scalable<double> StaticCooldown => 0;

    protected sealed override double DynamicUseTime(AbilityContext context) =>
        AttackInterval(context) * context.UserStats[Stat.AttackUseTimeFraction];
    
    protected sealed override double DynamicCooldown(AbilityContext context) =>
        AttackInterval(context) * (1 - context.UserStats[Stat.AttackUseTimeFraction]);

    private double AttackInterval(AbilityContext context) =>
        1 / (context.UserStats[Stat.AttackSpeed] / 1000);

    protected sealed override double DynamicRange(AbilityContext context) =>
        context.UserStats[Stat.AttackRange];
}
