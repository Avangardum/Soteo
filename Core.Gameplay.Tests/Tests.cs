using System.Reflection;
using Soteo.Core.Shared;

namespace Soteo.Core.Gameplay.Tests;

public abstract class Tests
{
    static Tests()
    {
        TypeLocator.Init(Assembly.GetExecutingAssembly(), CoreGameplayAssembly.Value);
    }
}
