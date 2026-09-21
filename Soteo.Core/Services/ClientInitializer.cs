using Soteo.Core.Interfaces;

namespace Soteo.Core.Services;

public sealed class ClientInitializer
{
    private readonly ISynchronizedCampaignStatePuppetRepository _synchronizedCampaignStateRepo;
    private readonly IInitializationRepository _initRepo;

    public ClientInitializer
    (
        ICampaignServerConnector campaignServerConnector,
        ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo,
        IInitializationRepository initRepo
    )
    {
        _synchronizedCampaignStateRepo = synchronizedCampaignStateRepo;
        _initRepo = initRepo;

        campaignServerConnector.Connected += OnConnected;
    }

    private void OnConnected()
    {
        InitAsync().CollectException();
    }

    private async Task InitAsync()
    {
        await _synchronizedCampaignStateRepo.WaitForInitAsync();
        _initRepo.IsInitialized = true;
    }
}
