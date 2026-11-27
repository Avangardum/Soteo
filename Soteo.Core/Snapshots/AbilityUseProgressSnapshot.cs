namespace Soteo.Core.Snapshots;

public sealed record AbilityUseProgressSnapshot
{
    public required AbilitySlot Slot { get; init; }
    public required AbilityContextSnapshot Context { get; init; }
    public double ElapsedTime { get; init; }
    public required double RemainingTime { get; init; }

    public double FullTime => ElapsedTime + RemainingTime;
    public double NormalizedProgress => FullTime == 0 ? 1 : ElapsedTime / FullTime;

    public static AbilityUseProgressSnapshot Interpolate
    (
        AbilityUseProgressSnapshot from,
        AbilityUseProgressSnapshot to,
        double weight
    )
    {
        return to with
        {
            ElapsedTime = Maths.LerpIncrease(from.ElapsedTime, to.ElapsedTime, weight),
            RemainingTime = Maths.LerpDecrease(from.RemainingTime, to.RemainingTime, weight)
        };
    }
}
