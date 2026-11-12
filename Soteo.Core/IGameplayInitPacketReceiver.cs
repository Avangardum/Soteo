namespace Soteo.Core.Interfaces;

public interface IGameplayInitPacketReceiver
{
    void ReceiveNoInitialSnapshotPacket();
    void ReceiveCampaignInitializedPacket();
}
