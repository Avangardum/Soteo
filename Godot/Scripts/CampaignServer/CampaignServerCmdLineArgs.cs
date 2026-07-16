using Soteo.Core.Enums;
using Soteo.Main.Gameplay;

namespace Soteo.Main.CampaignServer;

public static class CampaignServerCmdLineArgs
{
    public static IReadOnlyList<Guid> ShardIds { get; }
    public static bool IsSingleplayer { get; }
    
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
                shardIds.Add(Guid.Parse(args[i]));
            }
            else if (args[i] == "--singleplayer")
            {
                IsSingleplayer = true;
            }
            else
            {
                throw new ArgumentException($"Unsupported command line argument {args[i]}");
            }
        }
        
        ShardIds = shardIds;
    }
}
