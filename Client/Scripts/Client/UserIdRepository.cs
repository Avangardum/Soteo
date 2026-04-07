using Soteo.Client.Interfaces;
using Soteo.Client.Nodes;

namespace Soteo.Client;

public sealed class UserIdRepository : IUserIdRepository
{
    public UserIdRepository()
    {
        if (IsServer) UserId = Main.EditorIsServer ? Main.EditorLocalShardServerId : GetLocalShardServerId();
    }
    
    public Guid UserId { get; set; } // todo get client user id from token
    
    private Guid GetLocalShardServerId()
    {
        if (!IsServer) throw new InvalidOperationException();
        string[] args = OS.GetCmdlineArgs();
        int idIndex = args.IndexOf("--server") + 1;
        if (idIndex == 0 || idIndex == args.Length || !Guid.TryParse(args[idIndex], out var id))
            throw new ArgumentException("No server id found in args");
        return id;
    }
}