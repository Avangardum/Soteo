using Soteo.Core.Dto;

namespace Soteo.Core.Interfaces;

/// <inheritdoc cref="SynchronizedCampaignState"/>
public interface ISynchronizedCampaignStateRepository
{
    SynchronizedCampaignState Value { get; set; }
}
