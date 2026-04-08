using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Nodes;

namespace Soteo.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    extension (IServiceCollection self)
    {
        public IServiceCollection AddAlias<TAlias, TRefersTo>() where TRefersTo : TAlias where TAlias : class =>
            self.AddTransient<TAlias>(sp => sp.GetService<TRefersTo>()!);
        
        public IServiceCollection AddSingletonNode<TService, TImplementation>(string? path = null)
            where TImplementation : class, TService
            where TService : class
        {
            path ??= "Systems/" + typeof(TImplementation).Name;
            return self.AddSingleton<TService>(sp => sp.GetRequiredService<Main>().GetNode<TImplementation>(path));
        }

        public IServiceCollection AddShardScopedNode<TService, TImplementation>(string? path = null)
            where TImplementation : class, TService
            where TService : class
        {
            path ??= "Systems/" + typeof(TImplementation).Name;
            return self.AddScoped<TService>(sp => sp.GetRequiredService<Shard>().GetNode<TImplementation>(path));
        }
    }
}