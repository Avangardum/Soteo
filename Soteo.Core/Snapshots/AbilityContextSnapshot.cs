namespace Soteo.Core.Snapshots;

public sealed record AbilityContextSnapshot
{
    public required Ability Ability { get; init; }
    public required Guid UserId { get; init; }
    public bool Alt { get; init; }
    public required IReadOnlyDictionary<Stat, double> UserStats { get; init; }
    public AbilityTargetSnapshot Target { get; init; } = AbilityTargetSnapshot.Nothing;
}
