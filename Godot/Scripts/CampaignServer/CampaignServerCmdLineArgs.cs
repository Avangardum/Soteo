using Soteo.Core.Enums;
using Soteo.Main.Gameplay;

namespace Soteo.Main.CampaignServer;

public static class CampaignServerCmdLineArgs
{
    public static IReadOnlyList<Guid> ShardIds { get; }
    public static bool IsSingleplayer { get; }
    private static readonly int _port = 3706;
    private static readonly bool _isPortOverriden;
    public static int Port => _port;
    
    static CampaignServerCmdLineArgs()
    {
        if (SharedCmdLineArgs.Side != Side.CampaignServer)
            throw new InvalidOperationException("This class is for the campaign server only");
        
        string[] args = OS.GetCmdlineArgs();
        List<Guid> shardIds = [];
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--campaign-server")
            {
                
            }
            else if (args[i] == "--shard")
            {
                i++;
                if (i == args.Length)
                    throw new ArgumentException("--shard should be followed by a shard id");
                if (!Guid.TryParse(args[i], out Guid id))
                    throw new ArgumentException("Shard id must be a valid Guid");
                shardIds.Add(id);
            }
            else if (args[i] == "--singleplayer")
            {
                IsSingleplayer = true;
            }
            else if (args[i] == "--port")
            {
                if (_isPortOverriden)
                    throw new ArgumentException("Duplicate --port argument");
                i++;
                if (i == args.Length)
                    throw new ArgumentException("--port should be followed by a port number");
                if (!int.TryParse(args[i], out _port) || _port <= 0)
                    throw new ArgumentException("Port should be a positive integer");
                _isPortOverriden = true;
            }
            else
            {
                throw new ArgumentException($"Unsupported command line argument {args[i]}");
            }
        }
        
        ShardIds = shardIds;
    }
}
