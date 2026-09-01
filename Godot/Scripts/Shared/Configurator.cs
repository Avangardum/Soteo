using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Dto.Options;

namespace Soteo.Main.Shared;

public static class Configurator
{
    public static void RegisterConfigurationOptions(IServiceCollection services)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables("Soteo__")
            .Build();
        
        services.AddOptions<CampaignPersistenceOptions>().Bind(configuration).ValidateDataAnnotations();
        services.AddOptions<ToAuthServerConnectionOptions>().Bind(configuration).ValidateDataAnnotations();
        services.AddOptions<ToCampaignServerConnectionOptions>().Bind(configuration).ValidateDataAnnotations();
        services.AddOptions<CertificateVerificationOptions>().Bind(configuration).ValidateDataAnnotations();
        services.AddOptions<CertificateOptions>().Bind(configuration).ValidateDataAnnotations();
        services.AddOptions<IntercomOptions>().Bind(configuration).ValidateDataAnnotations();
    }
}
