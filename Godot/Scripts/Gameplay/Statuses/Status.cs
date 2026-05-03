using System.Collections.Immutable;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Statuses;

public abstract class Status
{
    private static readonly ImmutableDictionary<Type, Status> InstancesByType;

    public static ImmutableList<Status> All { get; }

    public static T Instance<T>() where T : Status => (T)InstancesByType[typeof(T)];

    static Status()
    {
        All = TypeLocator.InstanceAllSubclasses<Status>();
        InstancesByType = All.ToImmutableDictionary(it => it.GetType(), it => it);
    }
    
    public int Id => All.IndexOf(this);
    
    public abstract DuplicateStatusResolution DuplicateResolution { get; }
    
    public virtual IReadOnlyList<StatModifier> StatModifiers(StatusContext context) => [];
    
    public virtual void Tick(StatusContext context) { }
    
    public virtual void OnDealAttackDamage(StatusContext context, Unit target, double damage) { }
}