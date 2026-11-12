using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Interfaces;

public interface ICampaignSnapshotSerializer
{
    byte[] Serialize(CampaignSnapshot snapshot);
    CampaignSnapshot Deserialize(byte[] bytes);
}