using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes;

namespace Soteo.Gameplay;

public sealed class CurrentUserIdRepository : ICurrentUserIdRepository
{
    public CurrentUserIdRepository()
    {
        if (IsServer) UserId = GetLocalShardServerId();
    }
    
    public Guid UserId { get; set; } // todo get client user id from token
    
    private Guid GetLocalShardServerId()
    {
        if (!IsServer) throw new InvalidOperationException();
        if (Main.EditorIsServer) return Main.EditorLocalShardServerId;
        string[] args = OS.GetCmdlineArgs();
        int idIndex = args.IndexOf("--server") + 1;
        if (idIndex == 0 || idIndex == args.Length || !Guid.TryParse(args[idIndex], out var id))
            throw new ArgumentException("No server id found in args");
        return id;
    }
}