using System.Diagnostics.CodeAnalysis;

namespace Soteo.Core.Synchronization.Snapshots;

public class ProjectileTargetSnapshot
{
    public Guid? UnitId { get; }
    public Vector2? Position { get; }

    public ProjectileTargetSnapshot(Guid unitId) => UnitId = unitId;
    public ProjectileTargetSnapshot(Vector2 position) => Position = position;

    public static implicit operator ProjectileTargetSnapshot(Guid unitId) => new(unitId);
    public static implicit operator ProjectileTargetSnapshot(Vector2 position) => new(position);

    [MemberNotNullWhen(true, nameof(UnitId))]
    [MemberNotNullWhen(false, nameof(Position))]
    public bool IsUnit => UnitId != null;
}
