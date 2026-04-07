namespace Soteo.Client.Interfaces;

public interface IShardServiceProvider
{
    IServiceProvider? GetServiceProviderForShard(Guid id);
}