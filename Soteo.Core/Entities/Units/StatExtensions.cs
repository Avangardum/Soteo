
namespace Soteo.Core.Entities.Units;

public static class StatExtensions
{
    private static readonly IReadOnlyList<Stat> _all;
    private static readonly IReadOnlyList<Stat> _allResource;
    private static readonly IReadOnlyList<Stat> _allComputed;

    static StatExtensions()
    {
        _all = Enum.GetValues<Stat>().ToList().AsReadOnly();
        _allResource = _all.Where(it => it.IsResource).ToList().AsReadOnly();
        _allComputed = _all.Where(it => !it.IsResource).ToList().AsReadOnly();
    }

    extension (Stat self)
    {
        public static IReadOnlyList<Stat> All => _all;
        public static IReadOnlyList<Stat> AllResource => _allResource;
        public static IReadOnlyList<Stat> AllComputed => _allComputed;

        /// <summary>
        /// Resource stats are independent values and can change freely for any reason.
        /// Other stats are computed and cannot be changed directly.
        /// </summary>
        public bool IsResource => self is Stat.CurrentHealth or Stat.CurrentMana;
    }
}
