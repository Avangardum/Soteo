using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Dto.Options;
using Soteo.Core.Enums;
using Soteo.Main.Gameplay;

namespace Soteo.Main.Shared;

public static class Config
{
    public static void RegisterConfigurationOptions(IServiceCollection services)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables("Soteo__")
            .Build();
        
        // Register all options regardless of side, because omitting a registration would implicitly register a
        // default (possibly invalid) value which would be silently injected into IOptions<T> constructor parameters
        // instead of throwing an exception
        services.AddOptions<CampaignPersistenceOptions>().Bind(configuration).ValidateDataAnnotations();
        services.AddOptions<ToAuthServerConnectionOptions>().Bind(configuration).ValidateDataAnnotations();
        services.AddOptions<ToCampaignServerConnectionOptions>().Bind(configuration).ValidateDataAnnotations();
        services.AddOptions<CertificateVerificationOptions>().Bind(configuration).ValidateDataAnnotations();
        services.AddOptions<CertificateOptions>().Bind(configuration).ValidateDataAnnotations();
        services.AddOptions<IntercomOptions>().Bind(configuration).ValidateDataAnnotations();
        
        // Unwrap only the options that must be present for on current side. Options should be injected directly,
        // without using the IOptions<T> interface, this way trying to inject an option that's not supposed to be
        // defined on that side will throw even if it is coincidentally defined. The following registrations define
        // which sides can rely on which options.
        Side side = SharedCmdLineArgs.Side;
        if (side is Side.CampaignServer)
        {
            services.UnwrapOptions<CampaignPersistenceOptions>();
            services.UnwrapOptions<CertificateOptions>();
        }
        if (side is Side.Client or Side.ShardServer)
        {
            services.UnwrapOptions<ToAuthServerConnectionOptions>();
            services.UnwrapOptions<ToCampaignServerConnectionOptions>();
            services.UnwrapOptions<CertificateVerificationOptions>();
        }
        if (side is Side.ShardServer or Side.CampaignServer)
        {
            services.UnwrapOptions<IntercomOptions>();
        }
    }
}
