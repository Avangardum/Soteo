namespace Soteo.Core.Interfaces;

public interface ICampaignServerConnector
{
    event Action Connected;
    void ConnectAsPlayer(string email, string password);
    void ConnectAsShardServer();
}
