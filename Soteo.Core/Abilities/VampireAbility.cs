namespace Soteo.Core.Abilities;

public sealed class VampireAbility : Ability
{
    public override Targeting Targeting => Targeting.Passive;
    public override string IconPath => "Placeholder2";
    public override AbilityPassiveStatus PassiveStatus => Status.Instance<VampireStatus>();
    public double LifestealFactor => 0.4;
}
