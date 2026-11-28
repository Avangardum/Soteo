namespace Soteo.Core.Abilities;

public sealed class BuildTurretAbility : Ability
{
    public override Targeting Targeting => Targeting.Position;
    public override double StaticRange => 1;

    protected override void TakeEffectCore(AbilityContext context)
    {
        context.Services.EntityManager.SpawnTurret(context.Target.Position.Required());
    }
}
