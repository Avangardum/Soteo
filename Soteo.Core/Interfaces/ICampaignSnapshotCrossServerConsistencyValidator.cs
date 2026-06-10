using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Interfaces;

public interface ICampaignSnapshotCrossServerConsistencyValidator
{
    bool IsConsistent(CampaignSnapshot snapshot);
}
