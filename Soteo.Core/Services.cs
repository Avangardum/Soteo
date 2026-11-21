using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Snapshots;

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
    public ICrossInventoryHelper CrossInventoryHelper => _serviceProvider.GetRequiredService<ICrossInventoryHelper>();
    public ShardSnapshotArchive ShardSnapshotArchive => _serviceProvider.GetRequiredService<ShardSnapshotArchive>();
    public ICurrentTickRepository CurrentTickRepository =>
        _serviceProvider.GetRequiredService<ICurrentTickRepository>();
}
