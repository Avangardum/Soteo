using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.CampaignServerState.DataObjects;

public sealed record User
{
    public required Guid Id { get; init; }
    public required bool IsConnected { get; set; }
    public required bool IsPlayer { get; init; }
    public required bool IsShard { get; init; }
    
    public UserSnapshot CreateSnapshot()
    {
        return new UserSnapshot
        {
            Id = Id,
            IsConnected = IsConnected,
            IsPlayer = IsPlayer,
            IsShard = IsShard,
        };
    }
}
