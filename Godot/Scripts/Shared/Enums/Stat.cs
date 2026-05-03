using System.Collections.Immutable;
using Soteo.Shared.Extensions;

namespace Soteo.Shared.Enums;

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

public static class StatExtensions
{
    private static readonly ImmutableList<Stat> _all;
    private static readonly ImmutableList<Stat> _allVolatile;
    private static readonly ImmutableList<Stat> _allNonVolatile;
    
    static StatExtensions()
    {
        _all = Enum.GetValues<Stat>().ToImmutableList();
        _allVolatile = _all.Where(it => it.IsVolatile).ToImmutableList();
        _allNonVolatile = _all.Where(it => !it.IsVolatile).ToImmutableList();
    }
    
    extension (Stat self)
    {
        public static ImmutableList<Stat> All => _all;
        public static ImmutableList<Stat> AllVolatile => _allVolatile;
        public static ImmutableList<Stat> AllNonVolatile => _allNonVolatile;
        
        /// <summary>
        /// Volatile stats are independent values and can change freely for any reason.<br />
        /// Non-volatile stats cannot be changed directly, instead they are derived from
        /// default values, class, level and statuses.
        /// </summary>
        public bool IsVolatile => self is Stat.CurrentHealth or Stat.CurrentMana;
    }
}