namespace Soteo.Core.CampaignServer.Dto.Snapshots;

public sealed record UserSnapshot
{
    public required Guid Id { get; init; }
    public required bool IsConnected { get; init; }
    public required bool IsPlayer { get; init; }
    public required bool IsShard { get; init; }
}
