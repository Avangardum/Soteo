using Microsoft.Extensions.DependencyInjection;
using Soteo.CampaignServer;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared.Attributes;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Extensions;
using Soteo.Shared.Packets;
using Soteo.Util;
using Soteo.Util.Extensions;

namespace Soteo.Gameplay.PacketHandlers;

public sealed class RoutingPacketHandler
(
    IServiceProvider rootServiceProvider,
    IShardServiceProviderSource shardServiceProviderSource,
    ICurrentUserIdRepository currentUserIdRepository
) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        IServiceProvider? serviceProvider =
            Const.IsServer ? shardServiceProviderSource.ShardServiceProviders[currentUserIdRepository.UserId] :
            senderId == Const.CampaignServerId ? rootServiceProvider :
            shardServiceProviderSource.ShardServiceProviders[senderId];
        if (serviceProvider == null) return;

        if (!TypeLocator.PacketHandlerTypes.TryGetValue(packet.Type, out Type? handlerType))
            throw new BadPacketException($"Packet of type {packet.Type} can't be handled");
        if (Const.IsServer && senderId != Const.CampaignServerId && !handlerType.HasAttribute<AllowClientPacketsAttribute>())
            throw new BadPacketException($"Clients are not allowed to send packets of type {packet.Type}");
        var handler = (IPacketHandler)serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync(packet, senderId);
    }
}