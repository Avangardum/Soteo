namespace Soteo.Core.Interfaces;

public interface ICommunicator : IFromCampaignServerPacketSender
{
    void Poll();
}
