namespace Soteo.Gameplay.Interfaces;

public interface IShardServiceProvider
{
    IServiceProvider? GetServiceProviderForShard(Guid id);
}