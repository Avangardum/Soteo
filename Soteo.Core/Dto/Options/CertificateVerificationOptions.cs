namespace Soteo.Core.Dto.Options;

public sealed record CertificateVerificationOptions
{
    /// <summary>
    /// Whether TLS certificates of remote servers should be verified. Can be disabled in a development environment,
    /// where self-signed certificates are used, which would fail such verification. Must be enabled in production.
    /// </summary>
    public bool VerifyCertificate { get; init; } = true;
}
