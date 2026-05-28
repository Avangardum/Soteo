using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Exceptions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;
using Soteo.Util;

namespace Soteo.Core.Gameplay.PacketHandlers;

public sealed class RoutingPacketHandler
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

        if (!TypeLocator.PacketHandlerTypes.TryGetValue(packet.Type, out Type? handlerType))
        {
            Throw.PacketHandlerNotFound(packet.Type);
        }
        if
        (
            Const.IsServer &&
            senderId != Const.CampaignServerId &&
            !handlerType.HasAttribute<AllowClientPacketsAttribute>()
        )
        {
            Throw.ClientPacketsNotAllowed(handlerType);
        }
        
        var handler = (IPacketHandler)serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync(packet, senderId);
    }
}
