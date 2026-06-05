namespace Soteo.Core.Gameplay.Dto.Snapshots;

public sealed record ProjectileSnapshot : EntitySnapshot<ProjectileSnapshot>
{
    public required DeflatedAbilityContext AbilityContext { get; init; }
    public required double Speed { get; init; }
    public required DeflatedProjectileTarget Target { get; init; } 
    
    public override EntitySnapshot ToPuppet()
    {
        return new ProjectilePuppetSnapshot
        {
            Id = Id,
            IsRemoved = IsRemoved,
            Position = Position,
            Azimuth = Azimuth
        };
    }
    
    public override EntitySnapshotDelta DeltaFrom(ProjectileSnapshot? from) =>
        throw new NotSupportedException();
}
