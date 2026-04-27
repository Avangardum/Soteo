using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    extension (IServiceCollection self)
    {
        public IServiceProvider BuildAutofacServiceProvider()
        {
            // Autofac is used instead of built-in .NET DI container because the latter uses threading, which
            // breaks the engine in web export. https://github.com/godotengine/godot/issues/118124
            var builder = new ContainerBuilder();
            builder.Populate(self);
            IContainer container = builder.Build();
            return new AutofacServiceProvider(container);
        }
        
        public IServiceCollection AddAlias<TAlias, TRefersTo>() where TRefersTo : TAlias where TAlias : class =>
            self.AddTransient<TAlias>(sp => sp.GetService<TRefersTo>()!);
        
        public IServiceCollection AddSingletonNode<TService>(string path) where TService : class =>
            self.AddSingleton<TService>(sp => sp.GetRequiredService<Main>().GetNode<TService>(path));
        
        public IServiceCollection AddSingletonNode<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            return self.AddSingletonNode<TService>(typeof(TImplementation).Name);
        }

        public IServiceCollection AddShardScopedNode<TService>(string path) where TService : class =>
            self.AddScoped<TService>(sp => sp.GetRequiredService<IShard>().Node.GetNode<TService>(path));
        
        public IServiceCollection AddShardScopedNode<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            return self.AddShardScopedNode<TService>(typeof(TImplementation).Name);
        }
    }
}