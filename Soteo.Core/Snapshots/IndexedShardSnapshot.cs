namespace Soteo.Core.Snapshots;

/// <summary>
/// Wrapper around ShardSnapshot with indices of entities in it, allowing performant querying.
/// </summary>
public sealed class IndexedShardSnapshot
{
    private readonly ShardSnapshot _snapshot;
    private readonly IReadOnlyDictionary<Faction, IReadOnlyList<UnitSnapshot>> _unitsByFaction;

    public IndexedShardSnapshot(ShardSnapshot snapshot)
    {
        _snapshot = snapshot;
        Units = new UnitQueryBuilder(this);

        List<UnitSnapshot> units = _snapshot.Entities.Values.OfType<UnitSnapshot>().ToList();
        _unitsByFaction = Faction.All.ToDictionary
        (
            f => f,
            IReadOnlyList<UnitSnapshot> (f) => units.Where(u => u.Faction == f).ToList().AsReadOnly()
        ).AsReadOnly();
    }

    public long Tick => _snapshot.Tick;

    public UnitSnapshot? GetUnit(Guid id) => (UnitSnapshot?)GetEntity(id);

    public EntitySnapshot? GetEntity(Guid id) => _snapshot.Entities.GetValueOrDefault(id);

    public UnitQueryBuilder Units { get; }

    public class UnitQueryBuilder
    {
        private readonly IndexedShardSnapshot _snapshot;
        private readonly IReadOnlyList<Faction>? _factions;

        public UnitQueryBuilder(IndexedShardSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        private UnitQueryBuilder
        (
            IndexedShardSnapshot snapshot,
            IReadOnlyList<Faction>? factions
        )
        {
            _snapshot = snapshot;
            _factions = factions;
        }

        public UnitQueryBuilder InFaction(params IReadOnlyList<Faction> factions) => new(_snapshot, factions);

        public UnitQueryBuilder NotInFaction(params IReadOnlyList<Faction> factions) =>
            InFaction(Faction.All.Except(factions).ToList());

        private IEnumerable<UnitSnapshot> FilteredUnits()
        {
            IReadOnlyList<Faction> factions = _factions ?? Faction.All;
            return factions.SelectMany(f => _snapshot._unitsByFaction[f]);
        }

        public List<UnitSnapshot> ToList() => FilteredUnits().OrderBy(it => it.Id).ToList();

        public UnitSnapshot? ClosestTo(Vector2 position)
        {
            return FilteredUnits()
                .OrderBy(it => Vector2.Distance(it.Position, position))
                .ThenBy(it => it.Id)
                .FirstOrDefault();
        }
    }
}
