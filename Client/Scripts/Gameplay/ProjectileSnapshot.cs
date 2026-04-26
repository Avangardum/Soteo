namespace Soteo.Gameplay;

public sealed record ProjectileSnapshot(Guid Id) : EntitySnapshot<ProjectileSnapshot>(Id)
{
    public required AbilityContext.Deflated AbilityContext { get; init; }
    public required float Speed { get; init; }
}