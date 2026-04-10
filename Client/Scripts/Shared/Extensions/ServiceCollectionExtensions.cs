using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Nodes;

namespace Soteo.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    extension (IServiceCollection self)
    {
        public IServiceCollection AddAlias<TAlias, TRefersTo>() where TRefersTo : TAlias where TAlias : class =>
            self.AddTransient<TAlias>(sp => sp.GetService<TRefersTo>()!);
        
        public IServiceCollection AddSingletonNode<TService>(string path) where TService : class =>
            self.AddSingleton<TService>(sp => sp.GetRequiredService<Main>().GetNode<TService>(path));
        
        public IServiceCollection AddSingletonNode<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            return self.AddSingletonNode<TService>("Systems/" + typeof(TImplementation).Name); // todo rename systems to services
        }

        public IServiceCollection AddShardScopedNode<TService>(string path) where TService : class =>
            self.AddScoped<TService>(sp => sp.GetRequiredService<Shard>().GetNode<TService>(path));
        
        public IServiceCollection AddShardScopedNode<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            return self.AddShardScopedNode<TService>("Systems/" + typeof(TImplementation).Name);
        }
    }
}