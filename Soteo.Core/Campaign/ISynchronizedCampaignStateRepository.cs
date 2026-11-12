namespace Soteo.Core.Campaign;

/// <inheritdoc cref="SynchronizedCampaignState"/>
public interface ISynchronizedCampaignStateRepository
{
    SynchronizedCampaignState Value { get; set; }

    void Pause();
    void Unpause();
}
