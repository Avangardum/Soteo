namespace Soteo.Main.CampaignServer;

public static class EnvironmentVariables
{
    /// <summary>
    /// Secret used by servers for internal authentication and for token validation. Base64 string.
    /// Defined by all servers.
    /// </summary>
    public static string IntercomSecret =>
        SysEnvironment.GetRequiredEnvironmentVariable("Soteo__IntercomSecret");
    
    /// <summary>
    /// Path to the .crt file containing the certificate
    /// </summary>
    public static string CertificatePath => SysEnvironment.GetRequiredEnvironmentVariable("Soteo__CertificatePath");
    
    /// <summary>
    /// Path to the .key file containing the certificate private key
    /// </summary>
    public static string PrivateKeyPath => SysEnvironment.GetRequiredEnvironmentVariable("Soteo__PrivateKeyPath");
}
