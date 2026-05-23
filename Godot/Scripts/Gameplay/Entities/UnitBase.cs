using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Entities;

public abstract class UnitBase<TNode> : Entity<TNode> where TNode : Node2D, IEntityNode
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
            [Stat.MoveSpeed] = (5, 50, 500),
            [Stat.TurnSpeed] = (36, 360, 3600),
            [Stat.AttackDamage] = (0, 50, double.PositiveInfinity),
            [Stat.AttackSpeed] = (100, 1000, 10_000),
            [Stat.AttackUseTimeFraction] = (0, 0.5, 1),
            [Stat.AttackRange] = (10, 100, double.PositiveInfinity),
            [Stat.AttackProjectileSpeed] = (50, 500, 5000)
        }.ToImmutableDictionary();
    
    protected UnitBase(Guid id, TNode node) : base(id, node)
    {
        foreach (Stat stat in Stat.All)
            StatsInternal[stat] = StatConst[stat].Default;
        
        Faction = Id.GetHashCode() % 2 == 0 ? Faction.Empire : Faction.Syndicate;
    }
    
    public virtual bool IsDead { get; protected set; }
    protected bool IsMoving { get; set; }
    
    protected Dictionary<Stat, double> StatsInternal { get; set; } = [];
    public IReadOnlyDictionary<Stat, double> Stats => StatsInternal;
    
    protected Dictionary<AbilitySlot, AbilitySlotState> AbilitySlotStatesInternal { get; set; } = [];
    public IReadOnlyDictionary<AbilitySlot, AbilitySlotState> AbilitySlotStates => AbilitySlotStatesInternal;
    
    public AbilityUseProgress? AbilityUseProgress { get; protected set; }
    public Faction Faction { get; }
    
    public bool IsAlliedTo(UnitBase<TNode> other) => Faction != Faction.Neutral && other.Faction == Faction;
}