using Soteo.Core.Dto;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Repositories;

/// <inheritdoc cref="ISynchronizedCampaignStateRepository" />
public sealed class SynchronizedCampaignStatePuppetRepository :
    ISynchronizedCampaignStatePuppetRepository,
    ISynchronizedCampaignStatePacketReceiver
{
    public event Action Changed = delegate {};
    
    /// <inheritdoc/>
    public SynchronizedCampaignState? Value { get; private set; }

    public void ReceiveSynchronizedCampaignStatePacket(SynchronizedCampaignStatePacket packet)
    {
        if (Value == packet.Value) return;
        Value = packet.Value;
        Changed();
    }
}
