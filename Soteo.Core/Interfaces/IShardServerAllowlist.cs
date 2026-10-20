namespace Soteo.Core.Interfaces;

public interface IShardServerAllowlist
{
    bool IsEnabled { get; }
    IReadOnlyList<Guid> AllowedShardIds { get; }
}
