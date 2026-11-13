using Soteo.Core;
using Soteo.Core.Client;

namespace Soteo.Main.Gameplay;

public sealed class CurrentUserIdRepository : ICurrentUserIdRepository
{
    public CurrentUserIdRepository(ServerDependency<ShardOptions> shardOptions)
    {
        if (shardOptions.Value != null)
            Id = shardOptions.Value.ShardId;
    }

    public Guid? Id { get; set; }
}
