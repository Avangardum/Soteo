using Soteo.Core.Gameplay.Abilities;
using Soteo.Core.Gameplay.Statuses;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Dto;

public record PuppetStatusContext
{
    public required Guid Id { get; init; }
    public required Status Status { get; init; }
    public required Ability? Ability { get; init; }
    public required double ElapsedTime { get; init; }
    public required double DisplayElapsedTime { get; init; }
    public required double RemainingTime { get; init; }
    public required long Ordinal { get; init; }
    
    public double DisplayNormalizedRemainingTime
    {
        get
        {
            if (RemainingTime == double.PositiveInfinity) return 1;
            double totalDisplayTime = RemainingTime + DisplayElapsedTime;
            if (totalDisplayTime == 0) return 0;
            return RemainingTime / totalDisplayTime;
        }
    }
    
    public static PuppetStatusContext Interpolate(PuppetStatusContext from, PuppetStatusContext to, double weight)
    {
        return to with
        {
            DisplayElapsedTime = Maths.LerpIncrease(from.DisplayElapsedTime, to.DisplayElapsedTime, weight),
            RemainingTime = Maths.LerpDecrease(from.RemainingTime, to.RemainingTime, weight),
        };
    }
}
