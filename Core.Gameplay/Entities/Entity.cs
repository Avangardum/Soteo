using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Entities;

public abstract class Entity<TNode> : IEntity where TNode : class, IEntityNode
{
    protected Entity(Guid id, TNode node)
    {
        Id = id;
        Node = node;
        node.Entity = this;
    }

    public event Action Removed = delegate {};

    [MemberNotNullWhen(false, nameof(Node))] public bool IsRemoved { get; private set; }
    protected TNode? Node { get; private set; }
    public Guid Id { get; }
    public virtual Vector2 Position { get; set; }
    public virtual double Azimuth { get; set => field = Maths.PosMod(value, 360); }

    public abstract EntitySnapshot CreateSnapshot();

    public virtual void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        Position = snapshot.Position;
        Azimuth = snapshot.Azimuth;
    }
    
    public virtual void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight)
    {
        if (delta.Position.HasChanged)
            Position = Position.Lerp(delta.Position.NewValue, interpolationWeight);
        if (delta.Azimuth.HasChanged)
            Azimuth = Maths.ModularLerp(Azimuth, delta.Azimuth.NewValue, interpolationWeight, 360);
    }

    public virtual void Remove()
    {
        if (IsRemoved) return;
        IsRemoved = true;
        Node.Entity = null;
        Node = null;
        Removed();
        // Node is removed by EntityManager
    }
}
