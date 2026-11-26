using Soteo.Core.Deltas;
using Soteo.Core.Snapshots;

namespace Soteo.Core.Entities;

public abstract class Entity
{
    internal Entity(Guid id)
    {
        Id = id;
    }

    public event Action Removed = delegate { };

    public bool IsRemoved { get; protected set; }
    public Guid Id { get; }
    public virtual Vector2 Position { get; set; }
    public virtual double Azimuth { get; set => field = Maths.PosMod(value, 360); }

    public virtual void Tick(double delta) { }

    public abstract EntitySnapshot ToSnapshot();

    public virtual void ReplicateSnapshot(EntitySnapshot snapshot, Func<Guid, Entity> replicateEntityReference)
    {
        Position = snapshot.Position;
        Azimuth = snapshot.Azimuth;

        if (snapshot.IsRemoved)
            Remove();
    }

    public virtual void ApplyDelta(EntityDelta delta, double interpolationWeight)
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
        Removed();
    }
}

public abstract class Entity<TNode> : Entity where TNode : class, IEntityNode
{
    protected TNode? Node { get; private set; }

    internal Entity(Guid id, TNode node) : base(id)
    {
        Node = node;
        node.Entity = this;
    }

    public override void Remove()
    {
        if (IsRemoved) return;
        Node?.Entity = null;
        Node = null;
        base.Remove();
    }

    public void Respawn(TNode node)
    {
        if (!IsRemoved) throw new InvalidOperationException("Only removed entities can be respawned");
        Node = node;
        node.Entity = this;
        IsRemoved = false;
    }
}
