using System.Diagnostics.CodeAnalysis;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared;

namespace Soteo.Gameplay.Entities;

public abstract class Entity<TNode> : IEntity where TNode : Node2D, IEntityNode
{
    protected Entity(Guid id, TNode node, ClientDependency<ICamera> camera)
    {
        Id = id;
        
        Node = node;
        node.Entity = this;
        
        Camera = camera;
        Camera.Value?.ZoomChanged += OnZoomChanged;
    }

    // Both
    public event Action Removed = delegate {};

    // Client
    protected ClientDependency<ICamera> Camera { get; }
    // Both
    [MemberNotNullWhen(false, nameof(Node))] public bool IsRemoved { get; private set; }
    // Both
    protected TNode? Node { get; private set; }
    // Both
    public Guid Id { get; }
    // Both
    public virtual Vector2 Position { get; set; }
    // Both
    public virtual double Azimuth { get; set => field = Maths.PosMod(value, 360); }

    // Server
    public abstract EntitySnapshot CreateSnapshot();

    // Both
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

    // Both
    public void Remove()
    {
        if (IsRemoved) return;
        IsRemoved = true;
        Camera.Value?.ZoomChanged -= OnZoomChanged;
        Node.Entity = null;
        Node = null;
        Removed();
    }
    
    // Client
    protected virtual void OnZoomChanged() { }
}