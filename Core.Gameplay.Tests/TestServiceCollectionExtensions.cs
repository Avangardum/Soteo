using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Soteo.Core.Gameplay.Tests;

public static class TestServiceCollectionExtensions
{
    extension (IServiceCollection self)
    {
        public IServiceCollection AddSubstitute<T>() where T : class =>
            self.AddSingleton(Substitute.For<T>());
    }
}
