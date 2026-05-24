using Soteo.Core.Gameplay.Abilities;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Dto;

public record AbilitySlotState
{
    public required Ability Ability { get; init; }
    public required int Level { get; init; }
    public double Cooldown { get; init; }
    /// <summary>
    /// Cooldown of the ability at the last moment it was used
    /// </summary>
    public double MaxCooldown { get; init; }
    
    public static AbilitySlotState New<T>(int level) where T : Ability =>
        new AbilitySlotState { Ability = Ability.Instance<T>(), Level = level };
    
    public static AbilitySlotState Interpolate(AbilitySlotState from, AbilitySlotState to, double weight) =>
        to with { Cooldown = Maths.LerpDecrease(from.Cooldown, to.Cooldown, weight) };
}