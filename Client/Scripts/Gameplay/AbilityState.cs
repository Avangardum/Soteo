using Soteo.Gameplay.Abilities;

namespace Soteo.Gameplay;

public record AbilityState
{
    public required Ability Ability { get; init; }
    public required int Level { get; init; }
    public float Cooldown { get; init; }
    /// <summary>
    /// Cooldown of the ability at the last moment it was used
    /// </summary>
    public float MaxCooldown { get; init; }
    
    public static AbilityState New<T>(int level) where T : Ability<T>, new() =>
        new AbilityState { Ability = Ability<T>.Instance, Level = level };
}