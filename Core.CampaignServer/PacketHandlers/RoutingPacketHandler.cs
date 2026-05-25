using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;
using Soteo.Util;
using Soteo.Util.Extensions;

namespace Soteo.Core.CampaignServer.PacketHandlers;

public sealed class RoutingPacketHandler(IServiceProvider serviceProvider) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        if (!TypeLocator.PacketHandlerTypes.TryGetValue(packet.Type, out Type? handlerType))
            Throw.PacketHandlerNotFound(packet.Type);
        User sender = serviceProvider.GetRequiredService<IUserRepository>()[senderId];
        if (sender.IsPlayer && !handlerType.HasAttribute<AllowClientPacketsAttribute>())
            Throw.ClientPacketsNotAllowed(handlerType);
        var handler = (IPacketHandler)serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync(packet, senderId);
    }
}