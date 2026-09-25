using Soteo.Core.Dto;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Statuses;

public abstract class Status : SingletonHierarchyBase<Status>
{
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
