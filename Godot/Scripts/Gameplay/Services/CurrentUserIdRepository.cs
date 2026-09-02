using Soteo.Core.Dto.Options;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.SidedDependencies;

namespace Soteo.Main.Gameplay.Services;

public sealed class CurrentUserIdRepository : ICurrentUserIdRepository
{
    public CurrentUserIdRepository(ServerDependency<ShardOptions> shardOptions)
    {
        if (shardOptions.Value != null)
            Value = shardOptions.Value.ShardId;
    }
    
    public Guid? Value { get; set; }
    
    // todo remove
    public Guid Required => Value.Required;
}
