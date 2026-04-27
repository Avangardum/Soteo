namespace Soteo.Gameplay.Dto.Snapshots;

public sealed record ProjectileSnapshot : EntitySnapshot<ProjectileSnapshot>
{
    public required AbilityContext.Deflated AbilityContext { get; init; }
    public required float Speed { get; init; }
}