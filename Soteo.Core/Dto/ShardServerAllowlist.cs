using Soteo.Core.Interfaces;
using Soteo.Util;
using Soteo.Util.Interfaces;

namespace Soteo.Core.Dto;

public sealed class ShardServerAllowlist : IShardServerAllowlist
{
    public bool IsEnabled { get; }
    public IReadOnlySet<Guid> AllowedShardIds { get; }
    
    private ShardServerAllowlist(bool isEnabled, IReadOnlySet<Guid> allowedShardIds)
    {
        IsEnabled = isEnabled;
        AllowedShardIds = allowedShardIds;
    }
    
    public static ShardServerAllowlist Disabled() => new(false, new HashSet<Guid>().AsReadOnly());
    
    public static ShardServerAllowlist Enabled(IReadOnlyList<Guid> allowedShardIds) =>
        new(true, allowedShardIds.ToHashSet().AsReadOnly());
}
