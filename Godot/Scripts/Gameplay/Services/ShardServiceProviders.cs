using System.Collections;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Gameplay.Services;

public sealed class ShardServiceProviders(IReadOnlyDictionary<Guid, IServiceScope> scopes) : IShardServiceProviders
{
    private IReadOnlyDictionary<Guid, IServiceProvider> Providers =>
        scopes.ToImmutableDictionary(it => it.Key, it => it.Value.ServiceProvider);

    public IEnumerator<KeyValuePair<Guid, IServiceProvider>> GetEnumerator() => Providers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => Providers.Count;

    public bool ContainsKey(Guid key) => Providers.ContainsKey(key);

    public bool TryGetValue(Guid key, out IServiceProvider value) => Providers.TryGetValue(key, out value);

    public IServiceProvider this[Guid key] => Providers[key];

    public IEnumerable<Guid> Keys => Providers.Keys;

    public IEnumerable<IServiceProvider> Values => Providers.Values;
}
