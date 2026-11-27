namespace Soteo.Core.Abilities;

public sealed class SpawnDudesAbility : Ability
{
    public override Targeting Targeting => Targeting.Position;
    public override double StaticRange => double.PositiveInfinity;
    public override double StaticAngularRange => 180;

    protected override void TakeEffectCore(AbilityContext context)
    {
        for (double azimuth = 0; azimuth < 360; azimuth += 36)
        {
            Vector2 position = context.Target.Position.Required() + Maths.AzimuthToDirection(azimuth) * 3;
            context.Services.EntityManager.SpawnDude(position);
        }
    }
}
