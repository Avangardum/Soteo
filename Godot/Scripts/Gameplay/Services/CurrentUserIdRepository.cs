using Soteo.Core.Interfaces;

namespace Soteo.Main.Gameplay.Services;

public sealed class CurrentUserIdRepository : ICurrentUserIdRepository
{
    public CurrentUserIdRepository()
    {
        if (GameplayCmdLineArgs.IsServer)
            Value = ShardServerCmdLineArgs.ShardId;
    }
    
    public Guid? Value { get; set; }
    
    public Guid Required => Value.Required;
}
