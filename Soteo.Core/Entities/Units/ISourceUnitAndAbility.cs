namespace Soteo.Core.Entities.Units;

/// <summary>
/// Object containing information about a unit and ability being source of some effect.
/// Implemented by AbilityContext and StatusContext so that they can be passed as a single source object.
/// </summary>
public interface ISourceUnitAndAbility
{
    Unit? Unit { get; }
    Ability? Ability { get; }
    AbilityContext? AbilityContext { get; }
}
