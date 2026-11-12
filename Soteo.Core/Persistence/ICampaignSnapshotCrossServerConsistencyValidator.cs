namespace Soteo.Core.Persistence;

public interface ICampaignSnapshotCrossServerConsistencyValidator
{
    bool IsConsistent(CampaignSnapshot snapshot);
}
