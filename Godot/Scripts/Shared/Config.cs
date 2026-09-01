using Microsoft.Extensions.Configuration;

namespace Soteo.Main.Shared;

public static class Config
{
    public static IConfiguration Value { get; } =
        new ConfigurationBuilder()
            .AddEnvironmentVariables("Soteo__")
            .Build();
}
