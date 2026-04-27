using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Dto;

public sealed record AbilityUseProgress
{
    public required AbilitySlot Slot { get; init; }
    public float ElapsedTime { get; init; }
    public required float RemainingTime { get; init; }
    
    public float FullTime => ElapsedTime + RemainingTime;
    public float NormalizedProgress => FullTime == 0 ? 1 : ElapsedTime / FullTime;
    
    public AbilityUseProgress AddTime(float time)
    {
        return this with
        {
            ElapsedTime = ElapsedTime + time,
            RemainingTime = RemainingTime - time
        };
    }
}