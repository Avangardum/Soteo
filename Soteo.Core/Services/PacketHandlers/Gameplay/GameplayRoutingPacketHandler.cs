using Soteo.Core.Attributes;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

public sealed class GameplayRoutingPacketHandler
(
    IServiceProvider rootServiceProvider,
    IShardServiceProviders shardServiceProviders,
    ICurrentUserIdRepository currentUserIdRepository,
    ISideDetector sideDetector
) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        IServiceProvider? serviceProvider =
            sideDetector.IsServer ? shardServiceProviders[currentUserIdRepository.Required] :
            senderId == Const.CampaignServerId ? rootServiceProvider :
            shardServiceProviders[senderId];
        if (serviceProvider == null) return;
        
        IPacketHandler handler = serviceProvider.GetPacketHandlerFor<GameplayPacketHandlerAttribute>(packet.TypeCode) ??
            throw ExceptionFactory.PacketHandlerNotFound(packet.TypeCode, typeof(GameplayPacketHandlerAttribute));
        
        if
        (
            sideDetector.IsServer &&
            senderId != Const.CampaignServerId &&
            !handler.GetType().HasAttribute<AllowClientPacketsAttribute>()
        )
        {
            throw ExceptionFactory.ClientPacketsNotAllowed(handler.GetType());
        }
        
        await handler.HandleAsync(packet, senderId);
    }
}
