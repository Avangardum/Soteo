namespace Soteo.Core.Entities.Units;

/// <summary>
/// The base class for server-side Unit and client-side UnitPuppet
/// </summary>
public abstract class UnitBase<TNode> : Entity<TNode> where TNode : class, IEntityNode
{
    protected UnitBase(Guid id, TNode node) : base(id, node)
    {
        Faction = Faction.FromId(id);
    }

    public virtual bool IsDead { get; protected set; }
    protected bool IsMoving { get; set; }
    public AbilityUseProgress? AbilityUseProgress { get; protected set; }
    public Faction Faction { get; }
}
