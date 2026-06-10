using System.Collections.Immutable;

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
    /// Move speed in pixels per second
    /// </summary>
    MoveSpeed,
    /// <summary>
    /// Turn speed in degrees per second
    /// </summary>
    TurnSpeed,
    AttackDamage,
    /// <summary>
    /// Attack speed in millihertz (number of attacks per 1000 seconds)
    /// </summary>
    AttackSpeed, // todo use hertz
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

public static class StatExtensions
{
    private static readonly ImmutableList<Stat> _all;
    private static readonly ImmutableList<Stat> _allResource;
    private static readonly ImmutableList<Stat> _allComputed;
    
    static StatExtensions()
    {
        _all = Enum.GetValues<Stat>().ToImmutableList();
        _allResource = _all.Where(it => it.IsResource).ToImmutableList();
        _allComputed = _all.Where(it => !it.IsResource).ToImmutableList();
    }
    
    extension (Stat self)
    {
        public static ImmutableList<Stat> All => _all;
        public static ImmutableList<Stat> AllResource => _allResource;
        public static ImmutableList<Stat> AllComputed => _allComputed;
        
        /// <summary>
        /// Resource stats are independent values and can change freely for any reason.
        /// Other stats are computed and cannot be changed directly.
        /// </summary>
        public bool IsResource => self is Stat.CurrentHealth or Stat.CurrentMana;
    }
}
