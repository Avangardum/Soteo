using Soteo.Core.Snapshots;

namespace Soteo.Core.Entities.Units;

public sealed record AbilityUseProgress
{
    public required AbilitySlot Slot { get; init; }
    public required AbilityContext Context { get; init; }
    public double ElapsedTime { get; init; }
    public required double RemainingTime { get; init; }

    public double FullTime => ElapsedTime + RemainingTime;
    public double NormalizedProgress => FullTime == 0 ? 1 : ElapsedTime / FullTime;

    public AbilityUseProgress AddTime(double time)
    {
        return this with
        {
            ElapsedTime = ElapsedTime + time,
            RemainingTime = RemainingTime - time
        };
    }

    public AbilityUseProgressSnapshot ToSnapshot()
    {
        return new()
        {
            Slot = Slot,
            Context = Context.ToSnapshot(),
            ElapsedTime = ElapsedTime,
            RemainingTime = RemainingTime
        };
    }

    public static AbilityUseProgress FromSnapshot
    (
        AbilityUseProgressSnapshot snapshot,
        Services services,
        Func<Guid, Entity> replicateEntityReference
    )
    {
        return new()
        {
            Slot = snapshot.Slot,
            Context = AbilityContext.FromSnapshot(snapshot.Context, services, replicateEntityReference),
            ElapsedTime = snapshot.ElapsedTime,
            RemainingTime = snapshot.RemainingTime
        };
    }
}
