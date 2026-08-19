using Soteo.Core.Dto;

namespace Soteo.Core.Interfaces;

/// <inheritdoc cref="SynchronizedCampaignState"/>
public interface ISynchronizedCampaignStatePuppetRepository
{
    event Action Changed;
    
    /// <summary>
    /// Last known state. Can be null if disconnected or connected recently.
    /// Guaranteed to be not null while handling any packet except the one setting the initial state.
    /// </summary>
    SynchronizedCampaignState? Value { get; }
}
