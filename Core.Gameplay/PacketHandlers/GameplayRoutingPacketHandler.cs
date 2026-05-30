using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Extensions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.PacketHandlers;

public sealed class GameplayRoutingPacketHandler
(
    IServiceProvider rootServiceProvider,
    IShardServiceProviders shardServiceProviders,
    ICurrentUserIdRepository currentUserIdRepository
) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        IServiceProvider? serviceProvider =
            Const.IsServer ? shardServiceProviders[currentUserIdRepository.Required] :
            senderId == Const.CampaignServerId ? rootServiceProvider :
            shardServiceProviders[senderId];
        if (serviceProvider == null) return;
        
        IPacketHandler handler = serviceProvider.GetPacketHandlerFor(packet.TypeCode) ??
            throw ExceptionFactory.PacketHandlerNotFound(packet.TypeCode);
        
        if
        (
            Const.IsServer &&
            senderId != Const.CampaignServerId &&
            !handler.GetType().HasAttribute<AllowClientPacketsAttribute>()
        )
        {
            throw ExceptionFactory.ClientPacketsNotAllowed(handler.GetType());
        }
        
        await handler.HandleAsync(packet, senderId);
    }
}
