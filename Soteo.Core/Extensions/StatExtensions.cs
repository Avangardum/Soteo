using System.Collections.Immutable;
using Soteo.Core.Enums;

namespace Soteo.Core.Extensions;

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
