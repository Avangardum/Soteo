using System.Collections.Immutable;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared;

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
    
    /// <summary>
    /// Path to the status icon relative to res://Textures/Icons, without an extension.
    /// If null, uses the same icon as the ability that caused the status.
    /// </summary>
    public virtual string? IconPath => null;
    
    public virtual bool HudVisible => true;
    
    public virtual IReadOnlyList<StatModifier> StatModifiers(StatusContext context) => [];
    
    public virtual void Tick(StatusContext context) { }
    
    public virtual void OnDealAttackDamage(StatusContext context, Unit target, double damage) { }
    
    public string Description(ILocalizer localizer) =>
        localizer.GetString(GetType().Name.PascalCaseToSnakeCase().ToUpperInvariant() + "_DESCRIPTION");
}