namespace Soteo.Core;

public static class FactionExtensions
{
    private static readonly IReadOnlyList<Faction> AllCore = Enum.GetValues<Faction>();

    extension(Faction self)
    {
        public static IReadOnlyList<Faction> All => AllCore;

        public static Faction FromId(Guid id) => id.ToString()[^1] % 2 == 0 ? Faction.Empire : Faction.Syndicate;
    }
}
