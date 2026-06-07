using Soteo.Core.CampaignServer.Dto;
using Soteo.Core.CampaignServer.Interfaces;

namespace Soteo.Core.CampaignServer.Services;

public sealed class PersistenceService(IPacketSender packetSender)
{
    public async Task<CampaignSnapshot> SaveAsync()
    {
        throw new NotImplementedException();
    }
    
    public async Task LoadAsync(CampaignSnapshot snapshot)
    {
        throw new NotImplementedException();
    }
}
