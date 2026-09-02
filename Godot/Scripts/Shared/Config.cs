using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Dto.Options;
using Soteo.Core.Enums;
using Soteo.Main.Gameplay;

namespace Soteo.Main.Shared;

public static class Config
{
    private static readonly ImmutableDictionary<string, string?> Defaults = new Dictionary<string, string?>
    {
        ["Side"] = "Client",
        ["IsSingleplayer"] = "false",
        ["Environment"] = "Production",
        ["VerifyCertificate"] = "true",
        ["Port"] = "3706",
    }.ToImmutableDictionary(); // todo to appsettings.json
    
    private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(Defaults)
        .AddEnvironmentVariables("Soteo__")
        .AddCommandLine(OS.GetCmdlineArgs())
        .Build();
    
    // Static fields are only for accessing the configuration in main classes before a service provider is built
    internal static readonly Side Side = Side.Parse(Configuration["Side"].Required);
    internal static readonly bool IsSingleplayer = bool.Parse(Configuration["IsSingleplayer"].Required);
    
    public static void RegisterConfigurationOptions(IServiceCollection services)
    {
        // Register all options regardless of side, because omitting a registration would implicitly register a
        // default (possibly invalid) value which would be silently injected into IOptions<T> constructor parameters
        // instead of throwing an exception
        services.AddOptions<CampaignPersistenceOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<ToAuthServerConnectionOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<ToCampaignServerConnectionOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<CertificateVerificationOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<CertificateOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<IntercomOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<SideOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<SingleplayerOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<ShardOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<CameraOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<LogInOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<PortOptions>().Bind(Configuration).ValidateDataAnnotations();
        services.AddOptions<CampaignOptions>().Bind(Configuration).ValidateDataAnnotations();
        
        // Unwrap only the options that must be present on the current side. Options should be injected directly,
        // without using the IOptions<T> interface, this way trying to inject an option that's not supposed to be
        // defined on that side will throw even if it is coincidentally defined. The following registrations define
        // which sides can rely on which options.
        services.UnwrapOptions<SideOptions>();
        services.UnwrapOptions<SingleplayerOptions>();
        if (Side is Side.CampaignServer)
        {
            services.UnwrapOptions<CampaignPersistenceOptions>(); // todo multiplayer only
            services.UnwrapOptions<CertificateOptions>();
            services.UnwrapOptions<PortOptions>(); // todo multiplayer only
            services.UnwrapOptions<CampaignOptions>();
        }
        if (Side is Side.ShardServer)
        {
            services.UnwrapOptions<ShardOptions>();
        }
        if (Side is Side.Client)
        {
            services.UnwrapOptions<CameraOptions>();
            services.UnwrapOptions<LogInOptions>();
        }
        if (Side is Side.Client or Side.ShardServer)
        {
            services.UnwrapOptions<ToAuthServerConnectionOptions>();
            services.UnwrapOptions<ToCampaignServerConnectionOptions>();
            services.UnwrapOptions<CertificateVerificationOptions>();
        }
        if (Side is Side.ShardServer or Side.CampaignServer)
        {
            services.UnwrapOptions<IntercomOptions>();
        }
    }
}
