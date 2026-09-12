namespace Soteo.Core.Enums;

// TODO Use meters instead of pixels
// TODO Use radians instead of degrees
public enum Stat : byte
{
    MaxHealth,
    CurrentHealth,
    HealthRegen,
    MaxMana,
    CurrentMana,
    ManaRegen,
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
    /// Attack speed in hertz (attacks per second)
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
    /// Attack projectile speed in pixels per second
    /// </summary>
    AttackProjectileSpeed,
}
