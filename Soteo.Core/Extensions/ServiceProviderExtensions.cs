using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Interfaces;
using Soteo.Core.Services.Serializers.PacketSerializers;
using Soteo.Core.StaticHelpers;

namespace Soteo.Core.Extensions;

public static class ServiceProviderExtensions
{
    extension (IServiceProvider self)
    {
        public IPacketHandler? GetPacketHandlerFor<THandlerAttribute>(Type packetType)
            where THandlerAttribute : Attribute
        {
            var typeLocator = self.GetRequiredService<ITypeLocator>();
            return (IPacketHandler?)PacketHandlerLocator<THandlerAttribute>.TypeFor(packetType, typeLocator)
                ?.PassTo(self.GetRequiredService);
        }

        public IPacketSerializer? GetPacketSerializerFor(Type packetType)
        {
            var typeLocator = self.GetRequiredService<ITypeLocator>();
            return (IPacketSerializer?)PacketSerializer.TypeFor(packetType, typeLocator)
                ?.PassTo(self.GetRequiredService);
        }
    }
}
