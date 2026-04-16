using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay;

public record AbilityState : IReadOnlyAbilityState
{
    public AbilityState(Ability ability, int level)
    {
        Ability = ability;
        Level = level;
    }
    
    public AbilityState(IReadOnlyAbilityState other) : this(other.Ability, other.Level)
    {
        Cooldown = other.Cooldown;
    }
    
    public Ability Ability { get; }
    public int Level { get; set; }
    public float Cooldown { get; set; }

    public static AbilityState New<T>(int level) where T : Ability<T>, new() => new(Ability<T>.Instance, level);
}