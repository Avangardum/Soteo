namespace Soteo.Core.Networking;

public interface IFromCampaignServerCommunicator : IFromCampaignServerPacketSender, IConnectionNotifier
{
    void Poll();
}
