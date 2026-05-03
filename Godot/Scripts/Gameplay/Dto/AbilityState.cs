using Soteo.Gameplay.Abilities;

namespace Soteo.Gameplay.Dto;

public record AbilityState
{
    public required Ability Ability { get; init; }
    public required int Level { get; init; }
    public double Cooldown { get; init; }
    /// <summary>
    /// Cooldown of the ability at the last moment it was used
    /// </summary>
    public double MaxCooldown { get; init; }
    
    public static AbilityState New<T>(int level) where T : Ability =>
        new AbilityState { Ability = Ability.Instance<T>(), Level = level };
}