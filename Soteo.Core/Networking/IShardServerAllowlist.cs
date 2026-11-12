namespace Soteo.Core.Networking;

public interface IShardServerAllowlist
{
    bool IsEnabled { get; }
    IReadOnlyList<Guid> AllowedShardIds { get; }
}
