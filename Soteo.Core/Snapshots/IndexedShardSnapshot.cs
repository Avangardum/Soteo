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
        private readonly Shape? _shape;

        public UnitQueryBuilder(IndexedShardSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        private UnitQueryBuilder
        (
            IndexedShardSnapshot snapshot,
            IReadOnlyList<Faction>? factions,
            Shape? shape
        )
        {
            _snapshot = snapshot;
            _factions = factions;
            _shape = shape;
        }

        public UnitQueryBuilder InFaction(params IReadOnlyList<Faction> factions)
        {
            if (_factions != null)
                throw new InvalidOperationException("Faction filter is already set.");
            return new UnitQueryBuilder(_snapshot, factions, _shape);
        }

        public UnitQueryBuilder NotInFaction(params IReadOnlyList<Faction> factions) =>
            InFaction(Faction.All.Except(factions).ToList());

        public UnitQueryBuilder InCircle(Vector2 center, double radius)
        {
            if (double.IsNaN(center.X) || double.IsNaN(center.Y))
                throw new ArgumentException("center contains NaN.");
            if (double.IsNaN(radius))
                throw new ArgumentException("radius is NaN");
            if (radius < 0)
                throw new ArgumentException("radius is negative.");

            return InShape(new Circle(center, radius));
        }

        public UnitQueryBuilder InAabb(Vector2 from, Vector2 to)
        {
            if (double.IsNaN(from.X) || double.IsNaN(from.Y))
                throw new ArgumentException("from contains NaN");
            if (double.IsNaN(to.X) || double.IsNaN(to.Y))
                throw new ArgumentException("to contains NaN");
            if (from.X > to.X)
                throw new ArgumentException("from.X is greater than to.X");
            if (from.Y > to.Y)
                throw new ArgumentException("from.Y is greater than to.Y");

            return InShape(new Aabb(from, to));
        }

        private UnitQueryBuilder InShape(Shape shape)
        {
            if (_shape != null)
                throw new InvalidOperationException("Shape filter is already set.");
            return new UnitQueryBuilder(_snapshot, _factions, shape);
        }

        private IEnumerable<UnitSnapshot> FilteredUnits()
        {
            IEnumerable<UnitSnapshot> result =
                (_factions ?? Faction.All).SelectMany(f => _snapshot._unitsByFaction[f]);
            result = _shape switch
            {
                null => result,
                Aabb aabb => result
                    .Where(it => it.Position.X >= aabb.From.X)
                    .Where(it => it.Position.X <= aabb.To.X)
                    .Where(it => it.Position.Y >= aabb.From.Y)
                    .Where(it => it.Position.Y <= aabb.To.Y),
                Circle circle => result
                    .Where(it => Vector2.Distance(it.Position, circle.Center) <= circle.Radius)
            };
            return result;
        }

        public List<UnitSnapshot> ToList() => FilteredUnits().OrderBy(it => it.Id).ToList();

        public UnitSnapshot? ClosestTo(Vector2 position)
        {
            return FilteredUnits()
                .OrderBy(it => Vector2.Distance(it.Position, position))
                .ThenBy(it => it.Id)
                .FirstOrDefault();
        }

        private closed record Shape;
        private sealed record Aabb(Vector2 From, Vector2 To) : Shape;
        private sealed record Circle(Vector2 Center, double Radius) : Shape;
    }
}
