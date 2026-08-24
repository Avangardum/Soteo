using Soteo.Core.Dto;

namespace Soteo.Core.Interfaces;

/// <inheritdoc cref="SynchronizedCampaignState"/>
public interface ISynchronizedCampaignStatePuppetRepository
{
    event Action Changed;
    SynchronizedCampaignState Value { get; }
}
