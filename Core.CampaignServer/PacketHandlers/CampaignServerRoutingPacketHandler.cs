using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Extensions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.PacketHandlers;

public sealed class CampaignServerRoutingPacketHandler(IServiceProvider serviceProvider) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        IPacketHandler handler = serviceProvider.GetPacketHandlerFor(packet.TypeCode) ??
            throw ExceptionFactory.PacketHandlerNotFound(packet.TypeCode);
        User sender = serviceProvider.GetRequiredService<IUserRepository>()[senderId];
        if (sender.IsPlayer && !handler.GetType().HasAttribute<AllowClientPacketsAttribute>())
            throw ExceptionFactory.ClientPacketsNotAllowed(handler.GetType());
        await handler.HandleAsync(packet, senderId);
    }
}
