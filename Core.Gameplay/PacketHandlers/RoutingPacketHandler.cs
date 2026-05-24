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