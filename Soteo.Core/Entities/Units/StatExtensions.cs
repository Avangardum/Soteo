namespace Soteo.Core.Entities.Units;

public static class StatExtensions
{
    private static readonly IReadOnlyList<Stat> AllCore;
    private static readonly IReadOnlyList<Stat> AllResourceCore;
    private static readonly IReadOnlyList<Stat> AllComputedCore;

    private static readonly IReadOnlyDictionary<Stat, (double Min, double Default, double Max)> MinDefMax =
        new Dictionary<Stat, (double, double, double)>
        {
            [Stat.MaxHealth] = (0, 1000, 10_000),
            [Stat.CurrentHealth] = (0, 1000, 10_000),
            [Stat.HealthRegen] = (double.NegativeInfinity, 2, double.PositiveInfinity),
            [Stat.ManaRegen] = (double.NegativeInfinity, 2, double.PositiveInfinity),
            [Stat.MaxMana] = (0, 1000, 10_000),
            [Stat.CurrentMana] = (0, 1000, 10_000),
            [Stat.MoveSpeed] = (0.5, 5, 50),
            [Stat.TurnSpeed] = (36, 360, 3600),
            [Stat.AttackDamage] = (0, 50, double.PositiveInfinity),
            [Stat.AttackSpeed] = (0.1, 1, 10),
            [Stat.AttackUseTimeFraction] = (0, 0.5, 1),
            [Stat.AttackRange] = (1, 10, double.PositiveInfinity),
            [Stat.AttackProjectileSpeed] = (5, 50, 500)
        }.AsReadOnly();

    static StatExtensions()
    {
        AllCore = Enum.GetValues<Stat>().ToList().AsReadOnly();
        AllResourceCore = AllCore.Where(it => it.IsResource).ToList().AsReadOnly();
        AllComputedCore = AllCore.Where(it => !it.IsResource).ToList().AsReadOnly();
    }

    extension (Stat self)
    {
        public static IReadOnlyList<Stat> All => AllCore;
        public static IReadOnlyList<Stat> AllResource => AllResourceCore;
        public static IReadOnlyList<Stat> AllComputed => AllComputedCore;

        /// <summary>
        /// Resource stats are independent values and can change freely for any reason.
        /// Other stats are computed and cannot be changed directly.
        /// </summary>
        public bool IsResource => self is Stat.CurrentHealth or Stat.CurrentMana;

        public double Min => MinDefMax[self].Min;
        public double Default => MinDefMax[self].Default;
        public double Max => MinDefMax[self].Max;
    }
}
