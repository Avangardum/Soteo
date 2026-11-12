using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Networking;

public sealed record CertificateOptions
{
    [Required]
    public required string CertificatePath { get; init; }

    [Required]
    public required string PrivateKeyPath { get; init; }
}
