using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Interfaces;

namespace Soteo.Core.Shared.Extensions;

public static class ServiceProviderExtensions
{
    extension (IServiceProvider self)
    {
        public IPacketHandler? GetPacketHandlerFor(PacketType packetType) =>
            (IPacketHandler?)PacketHandler.TypeFor(packetType)?.PassTo(self.GetRequiredService);
    }
}
