namespace Soteo.Core.Entities.Units;

public record AbilitySlotState(Ability Ability)
{
    public double Cooldown { get; init; }

    /// <summary>
    /// Cooldown of the ability at the last moment it was used. Used for cooldown indicators.
    /// </summary>
    public double MaxCooldown { get; init; }

    public static AbilitySlotState New<T>() where T : Ability, new() => new(Ability.Instance<T>());

    public static AbilitySlotState Interpolate(AbilitySlotState from, AbilitySlotState to, double weight) =>
        to with { Cooldown = Maths.LerpDecrease(from.Cooldown, to.Cooldown, weight) };
}
