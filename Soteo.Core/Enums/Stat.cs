namespace Soteo.Core.Enums;

public enum Stat : byte
{
    MaxHealth,
    CurrentHealth,
    HealthRegen,
    MaxMana,
    CurrentMana,
    ManaRegen,
    /// <summary>
    /// Move speed in meters per second
    /// </summary>
    MoveSpeed,
    /// <summary>
    /// Turn speed in degrees per second
    /// </summary>
    TurnSpeed,
    AttackDamage,
    /// <summary>
    /// Attack speed in hertz (attacks per second)
    /// </summary>
    AttackSpeed,
    /// <summary>
    /// Use time of an attack ability as a fraction of attack interval. The rest is attack cooldown.
    /// </summary>
    AttackUseTimeFraction,
    /// <summary>
    /// Attack range in meters
    /// </summary>
    AttackRange,
    /// <summary>
    /// Attack projectile speed in meters per second
    /// </summary>
    AttackProjectileSpeed,
}
