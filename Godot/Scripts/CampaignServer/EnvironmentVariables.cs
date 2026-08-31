namespace Soteo.Main.CampaignServer;

public static class EnvironmentVariables
{
    public static string CampaignSnapshotPath =>
        SysEnvironment.GetRequiredEnvironmentVariable("Soteo__CampaignSnapshotPath");
    
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
    
    /// <summary>
    /// Whether certificates of remote servers are verified when making requests. True by default.
    /// Can be set to false for development testing where using a self-signed development certificate would fail
    /// validation. Must be true in production.
    /// </summary>
    public static bool VerifyCertificate =>
        SysEnvironment.GetEnvironmentVariable("Soteo__VerifyCertificate")?.PassTo(bool.Parse) ?? true;
}
