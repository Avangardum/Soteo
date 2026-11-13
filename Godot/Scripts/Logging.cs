using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Soteo.Main.Shared;

public static class Logging
{
    public static void AddToServiceCollection(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton<ILoggerProvider>(new GodotLogger.Provider());
    }
}
