using System.Reflection;
using Soteo.Core.Shared;

namespace Soteo.Core.Gameplay.Tests;

public abstract class CoreGameplayTests
{
    static CoreGameplayTests()
    {
        TypeLocator.Init(Assembly.GetExecutingAssembly(), CoreGameplayAssembly.Value, CoreSharedAssembly.Value);
    }
}
