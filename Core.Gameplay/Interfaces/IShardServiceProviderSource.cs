namespace Soteo.Gameplay.Interfaces;

public interface IShardServiceProviderSource
{
    IReadOnlyDictionary<Guid, IServiceProvider> ShardServiceProviders { get; }
}