namespace Soteo.Core.Campaign;

/// <inheritdoc cref="SynchronizedCampaignState"/>
public interface ISynchronizedCampaignStatePuppetRepository
{
    event Action Changed;
    SynchronizedCampaignState State { get; }
    Task WaitForInitAsync();
}
