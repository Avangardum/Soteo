using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Dto.Options;
using Soteo.Core.Enums;
using Soteo.Main.Gameplay;

namespace Soteo.Main.Shared;

internal static class Config
{
    // The game uses .NET configuration and option APIs for configuring the app. Configuration can be specified in the
    // following ways:
    // - appsettings.json
    // - appsettings.<Environment>.json (for example, appsettings.Development.json defines configuration specific for
    // development)
    // - environment variables prefixed with Soteo__ (use the __ separator to declare hierarchical values, for
    // example, Soteo__Foo__Bar sets value Bar in section Foo)
    // - command line arguments in the Key=Value format (for distinction against Godot arguments, which use the
    // --key value format) (use the : separator to declare hierarchical values)

    private static readonly string Environment = BuildConfiguration(null)["Environment"].Required;
    private static readonly IConfiguration Configuration = BuildConfiguration(Environment);

    // Static fields are only for accessing the configuration in main classes before a service provider is built
    public static readonly Side Side = Side.Parse(Configuration["Side"].Required);

    private static IConfiguration BuildConfiguration(string? environment)
    {
        var builder = new ConfigurationBuilder();
        builder.AddGodotJsonFile("res://appsettings.json", optional: false);
        if (environment != null)
            builder.AddGodotJsonFile($"res://appsettings.{environment}.json", optional: true);
        builder.AddCommandLine(OS.GetCmdlineArgs());
        builder.AddEnvironmentVariables("Soteo__");
        return builder.Build();
    }

    public static void AddToServiceCollection(IServiceCollection services)
    {
        void Add<T>(params IReadOnlyList<Side> sides) where T : class
        {
            // Register all options regardless of side, because omitting a registration would implicitly register a
            // default (possibly invalid) value which would be silently injected into IOptions<T> constructor parameters
            // instead of throwing an exception
            services.AddOptions<T>().Bind(Configuration).ValidateDataAnnotations();

            // Unwrap (register as plain object without IOptions<T> wrapper) only the options that must be present on
            // the current side. Options should be injected directly, without using IOptions<T> interface, this way
            // trying to inject an option that's not supposed to be defined on that side will throw even if it is
            // coincidentally defined.
            if (sides.Contains(Side))
                services.UnwrapOptions<T>();
        }

        Add<CampaignPersistenceOptions>(Side.CampaignServer);
        Add<ToAuthServerConnectionOptions>(Side.Client, Side.ShardServer);
        Add<ToCampaignServerConnectionOptions>(Side.Client, Side.ShardServer);
        Add<CertificateVerificationOptions>(Side.Client, Side.ShardServer);
        Add<CertificateOptions>(Side.CampaignServer);
        Add<ServerOptions>(Side.Client, Side.ShardServer, Side.CampaignServer);
        Add<SideOptions>(Side.Client, Side.ShardServer, Side.CampaignServer);
        Add<ShardOptions>(Side.ShardServer);
        Add<CameraOptions>(Side.Client);
        Add<LogInOptions>(Side.Client);
        Add<PortOptions>(Side.CampaignServer);
        Add<CampaignOptions>(Side.CampaignServer);
    }
}
