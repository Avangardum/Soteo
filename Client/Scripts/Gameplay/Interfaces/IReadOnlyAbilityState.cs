using Soteo.Gameplay.Abilities;

namespace Soteo.Gameplay.Interfaces;

public interface IReadOnlyAbilityState
{
    Ability Ability { get; }
    int Level { get; }
    float Cooldown { get; }
}