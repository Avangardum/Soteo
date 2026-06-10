using Soteo.Core.Dto;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Statuses;

public abstract class Status
{
    private static readonly Dictionary<Type, Status> Instances = [];
    private static Type? _currentlyConstructedType;
    
    public static T Instance<T>() where T : Status, new() => (T)Instance(typeof(T));
    
    public static Status Instance(Type type)
    {
        if (!type.IsAssignableTo(typeof(Status)))
            throw new ArgumentException($"{type} is not a status");
        
        if (Instances.TryGetValue(type, out Status existingStatus))
            return existingStatus;
        
        _currentlyConstructedType = type;
        var newInstance = (Status)Activator.CreateInstance(type);
        _currentlyConstructedType = null;
        Instances[type] = newInstance;
        return newInstance;
    }
    
    protected Status()
    {
        if (GetType() != _currentlyConstructedType)
        {
            throw new InvalidOperationException
            (
                "Statuses should not be created with new, use Status.Instance instead"
            );
        }
    }
    
    public abstract DuplicateStatusResolution DuplicateResolution { get; }
    
    /// <summary>
    /// Path to the status icon relative to res://Textures/Icons, without an extension.
    /// If null, uses the same icon as the ability that caused the status.
    /// </summary>
    public virtual string? IconPath => null;
    
    public virtual bool HudVisible => true;
    
    public virtual IReadOnlyList<StatModifier> StatModifiers(StatusContext context) => [];
    
    public virtual void Tick(StatusContext context, double delta) { }
    
    public virtual void OnDealAttackDamage(StatusContext context, IUnit target, double damage) { }
    
    public string Description(ILocalizer localizer) =>
        localizer.GetString(GetType().Name.PascalCaseToSnakeCase().ToUpperInvariant() + "_DESCRIPTION");
}
