using Soteo.Gameplay.Abilities;

namespace Soteo.Gameplay;

public sealed record ProjectileSnapshot(Guid Id) : EntitySnapshot<ProjectileSnapshot>(Id)
{
    // todo serialize the following
    public Guid? SourceId { get; init; }
    public Guid? TargetId { get; init; }
    public Ability? Ability { get; init; }
    public float? Speed { get; init; }
}