using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

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
            sideDetector.Side == Side.ShardServer ? shardServiceProviders[currentUserIdRepository.Required] :
            senderId == Const.CampaignServerId ? rootServiceProvider :
            shardServiceProviders.GetOrDefault(senderId);
        if (serviceProvider == null) return;
        
        IPacketHandler handler = serviceProvider.GetPacketHandlerFor<GameplayPacketHandlerAttribute>(packet.TypeCode) ??
            throw ExceptionFactory.PacketHandlerNotFound(packet.TypeCode, typeof(GameplayPacketHandlerAttribute));
        
        if
        (
            sideDetector.Side == Side.ShardServer &&
            senderId != Const.CampaignServerId &&
            !handler.GetType().HasAttribute<AllowClientPacketsAttribute>()
        )
        {
            throw ExceptionFactory.ClientPacketsNotAllowed(handler.GetType());
        }
        
        await handler.HandleAsync(packet, senderId);
    }
}
