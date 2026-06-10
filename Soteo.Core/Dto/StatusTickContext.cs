namespace Soteo.Core.Dto;

public sealed record StatusTickContext
{
    public required double Interval { get; init; }
    public required double Countdown { get; init; }
}
