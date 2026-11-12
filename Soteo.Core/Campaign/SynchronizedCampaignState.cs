using Soteo.Core.Services.Repositories;

namespace Soteo.Core.Dto;

/// <summary>
/// Campaign state synchronized across all servers and clients.
/// The campaign server can read and write this state via <see cref="SynchronizedCampaignStateRepository"/>.
/// Shard servers and clients can read it via <see cref="SynchronizedCampaignStatePuppetRepository"/>.
/// </summary>
public sealed record SynchronizedCampaignState
{
    public bool IsPaused { get; init; } = true;
}
