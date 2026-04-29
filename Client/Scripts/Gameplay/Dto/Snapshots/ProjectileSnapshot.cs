namespace Soteo.Gameplay.Dto.Snapshots;

public sealed record ProjectileSnapshot : EntitySnapshot<ProjectileSnapshot>
{
    public required DeflatedAbilityContext AbilityContext { get; init; }
    public required float Speed { get; init; }
}