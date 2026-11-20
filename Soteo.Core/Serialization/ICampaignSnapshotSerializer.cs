using Soteo.Core.Snapshots;

namespace Soteo.Core.Serialization;

public interface ICampaignSnapshotSerializer
{
    byte[] Serialize(CampaignSnapshot snapshot);
    CampaignSnapshot Deserialize(byte[] bytes);
}