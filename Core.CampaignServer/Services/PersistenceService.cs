using System.Collections.Immutable;
using Soteo.Core.CampaignServer.Dto;
using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared.Dto.Snapshots;

namespace Soteo.Core.CampaignServer.Services;

public sealed class PersistenceService
(
    IPacketSender packetSender,
    IUserRepository userRepo,
    IPlayerCharacterRepository charRepo
)
{
    public async Task<CampaignSnapshot> SaveAsync()
    {
        return new CampaignSnapshot
        {
            CampaignServer = new CampaignServerSnapshot
            {
                Characters = charRepo.CreateSnapshot(),
                Users = userRepo.CreateSnapshot(),
            },
            Shards = ImmutableDictionary<Guid, ShardSnapshot>.Empty,
        };
    }
    
    public async Task LoadAsync(CampaignSnapshot snapshot)
    {
        throw new NotImplementedException();
    }
}
