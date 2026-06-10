using Soteo.Core.CampaignServer.Dto.Snapshots;

namespace Soteo.Core.CampaignServer.Interfaces;

public interface ICampaignSnapshotCrossServerConsistencyValidator
{
    bool IsConsistent(CampaignSnapshot snapshot);
}
