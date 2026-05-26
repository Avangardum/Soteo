using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Gameplay.Interfaces;
using Soteo.Util;

namespace Soteo.Gameplay.Services;

public sealed class CurrentUserIdRepository : ICurrentUserIdRepository
{
    public CurrentUserIdRepository()
    {
        if (Const.IsServer) UserId = GetLocalShardServerId();
    }
    
    public Guid UserId { get; set; }
    
    private Guid GetLocalShardServerId()
    {
        if (!Const.IsServer) throw new InvalidOperationException();
        if (Main.EditorIsServer) return Main.EditorLocalShardServerId;
        
        string[] args = OS.GetCmdlineArgs();
        int idIndex = args.IndexOf("--server") + 1;
        if (idIndex == 0 || idIndex == args.Length || !Guid.TryParse(args[idIndex], out var id))
            throw new Exception("No server id found in args");
        return id;
    }
}
