using System.Collections.Immutable;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;

namespace Soteo.Core.Gameplay.Statuses;

public abstract class Status
{
    private static readonly ImmutableDictionary<Type, Status> InstancesByType;

    public static ImmutableList<Status> All { get; }

    public static T Instance<T>() where T : Status
    {
        if (InstancesByType.TryGetValue(typeof(T), out Status? instance))
            return (T)instance;
        throw ExceptionFactory.TypeNotFound(typeof(T));
    }

    static Status()
    {
        All = TypeLocator.InstanceAllSubclasses<Status>();
        InstancesByType = All.ToImmutableDictionary(it => it.GetType(), it => it);
    }
    
    public int Id => All.IndexOf(this);
    
    public abstract DuplicateStatusResolution DuplicateResolution { get; }
    
    /// <summary>
    /// Path to the status icon relative to res://Textures/Icons, without an extension.
    /// If null, uses the same icon as the ability that caused the status.
    /// </summary>
    public virtual string? IconPath => null;
    
    public virtual bool HudVisible => true;
    
    public virtual IReadOnlyList<StatModifier> StatModifiers(StatusContext context) => [];
    
    public virtual void Tick(StatusContext context, double delta) { }
    
    public virtual void OnDealAttackDamage(StatusContext context, Unit target, double damage) { }
    
    public string Description(ILocalizer localizer) =>
        localizer.GetString(GetType().Name.PascalCaseToSnakeCase().ToUpperInvariant() + "_DESCRIPTION");
}
