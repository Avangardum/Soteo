using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core;

/// <summary>
/// Collection of services included in AbilityContext and StatusContext
/// </summary>
public sealed class Services
{
    private readonly IServiceProvider _serviceProvider;

    public Services(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEntityManager EntityManager => _serviceProvider.GetRequiredService<IEntityManager>();
    public IFromGameplayPacketSender PacketSender => _serviceProvider.GetRequiredService<IFromGameplayPacketSender>();
}
