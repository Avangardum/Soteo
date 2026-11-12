namespace Soteo.Core.Entities.Units;

/// <summary>
/// The base class for server-side Unit and client-side UnitPuppet
/// </summary>
public abstract class UnitBase<TNode> : Entity<TNode> where TNode : class, IEntityNode
{
    public static readonly IReadOnlyDictionary<Stat, (double Min, double Default, double Max)> StatConst =
        new Dictionary<Stat, (double, double, double)>
        {
            [Stat.MaxHealth] = (0, 1000, 10_000),
            [Stat.CurrentHealth] = (0, 1000, 10_000),
            [Stat.HealthRegen] = (double.NegativeInfinity, 2, double.PositiveInfinity),
            [Stat.ManaRegen] = (double.NegativeInfinity, 2, double.PositiveInfinity),
            [Stat.MaxMana] = (0, 1000, 10_000),
            [Stat.CurrentMana] = (0, 1000, 10_000),
            [Stat.MoveSpeed] = (0.5, 5, 50),
            [Stat.TurnSpeed] = (36, 360, 3600),
            [Stat.AttackDamage] = (0, 50, double.PositiveInfinity),
            [Stat.AttackSpeed] = (0.1, 1, 10),
            [Stat.AttackUseTimeFraction] = (0, 0.5, 1),
            [Stat.AttackRange] = (1, 10, double.PositiveInfinity),
            [Stat.AttackProjectileSpeed] = (5, 50, 500)
        }.ToDictionary().AsReadOnly();

    protected UnitBase(Guid id, TNode node) : base(id, node)
    {
        Faction = Id.ToString()[^1] % 2 == 0 ? Faction.Empire : Faction.Syndicate;
    }

    public virtual bool IsDead { get; protected set; }
    protected bool IsMoving { get; set; }
    public AbilityUseProgress? AbilityUseProgress { get; protected set; }
    public Faction Faction { get; }
}
