using Soteo.Core.Gameplay.Abilities;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Entities;

namespace Soteo.Core.Gameplay.Interfaces;

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
