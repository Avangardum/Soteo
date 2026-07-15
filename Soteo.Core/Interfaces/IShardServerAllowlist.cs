using Soteo.Util.Interfaces;

namespace Soteo.Core.Interfaces;

public interface IShardServerAllowlist
{
    public bool IsEnabled { get; }
    public IReadOnlySet<Guid> AllowedShardIds { get; }
}
