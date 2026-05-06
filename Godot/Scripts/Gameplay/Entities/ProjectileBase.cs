using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Entities;

public abstract class ProjectileBase<TNode> : Entity<TNode> where TNode : Node2D, IEntityNode
{
    protected ProjectileBase
    (
        Guid id,
        TNode node,
        ClientDependency<ICamera> camera
    ) : base(id, node, camera) { }
    
    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        var s = (ProjectileSnapshot)snapshot;
        Position = s.Position;
        Azimuth = s.Azimuth;
    }
}