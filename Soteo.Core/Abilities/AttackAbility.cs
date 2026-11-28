namespace Soteo.Core.Abilities;

public abstract class AttackAbility : Ability
{
    public override Targeting Targeting => Targeting.Enemy | Targeting.Character | Targeting.Building;
    public sealed override double StaticRange => 0;
    public sealed override double StaticUseTime => 0;
    public sealed override double StaticCooldown => 0;

    protected sealed override double DynamicUseTime(AbilityContext context) =>
        AttackInterval(context) * context.User.GetStat(Stat.AttackUseTimeFraction);

    protected sealed override double DynamicCooldown(AbilityContext context) =>
        AttackInterval(context) * (1 - context.User.GetStat(Stat.AttackUseTimeFraction));

    private double AttackInterval(AbilityContext context) =>
        1 / context.User.GetStat(Stat.AttackSpeed);

    protected sealed override double DynamicRange(AbilityContext context) =>
        context.User.GetStat(Stat.AttackRange);
}
