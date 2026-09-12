namespace Soteo.Core.Interfaces;

public interface IFromCampaignServerCommunicator : IFromCampaignServerPacketSender, IConnectionNotifier
{
    void Poll();
}
