namespace Soteo.Shared.Enums;

public enum Stat : byte
{
    MaxHealth,
    CurrentHealth,
    MaxMana,
    CurrentMana,
    /// <summary>
    /// Move speed in pixels per second
    /// </summary>
    MoveSpeed,
    /// <summary>
    /// Turn speed in degrees per second
    /// </summary>
    TurnSpeed,
    AttackDamage,
    /// <summary>
    /// Attack speed in Millihertz (number of attacks per 1000 seconds)
    /// </summary>
    AttackSpeed,
    /// <summary>
    /// Use time of an attack ability as a fraction of attack interval. The rest is attack cooldown.
    /// </summary>
    AttackUseTimeFraction,
    /// <summary>
    /// Attack range in pixels
    /// </summary>
    AttackRange,
    /// <summary>
    /// Attack projectile speed in pixels per seconds
    /// </summary>
    AttackProjectileSpeed
}