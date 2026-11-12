namespace Soteo.Core;

public interface IGameplayInitPacketReceiver
{
    void ReceiveNoInitialSnapshotPacket();
    void ReceiveCampaignInitializedPacket();
}
