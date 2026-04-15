using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay;

public sealed record AbilityCastContext(int Level, Unit Caster, IServiceProvider Services) : IServiceProvider
{
    public object GetService(Type serviceType) => Services.GetService(serviceType);
}