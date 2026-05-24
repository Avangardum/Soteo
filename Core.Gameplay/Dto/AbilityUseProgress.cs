using Soteo.Core.Gameplay.Enums;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Dto;

public sealed record AbilityUseProgress
{
    public required AbilitySlot Slot { get; init; }
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
    
    public static AbilityUseProgress Interpolate(AbilityUseProgress from, AbilityUseProgress to, double weight)
    {
        return to with
        {
            ElapsedTime = Maths.LerpIncrease(from.ElapsedTime, to.ElapsedTime, weight),
            RemainingTime = Maths.LerpDecrease(from.RemainingTime, to.RemainingTime, weight),
        };
    }
}