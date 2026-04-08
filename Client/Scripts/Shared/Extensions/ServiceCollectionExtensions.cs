using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    extension (IServiceCollection self)
    {
        public IServiceCollection AddAlias<TAlias, TRefersTo>() where TRefersTo : TAlias where TAlias : class =>
            self.AddTransient<TAlias>(sp => sp.GetService<TRefersTo>()!);
    }
}