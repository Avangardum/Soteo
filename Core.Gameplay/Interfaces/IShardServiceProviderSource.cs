namespace Soteo.Core.Gameplay.Interfaces;

public interface IShardServiceProviderSource // todo inherit from dictionary
{
    IReadOnlyDictionary<Guid, IServiceProvider> ShardServiceProviders { get; }
}
