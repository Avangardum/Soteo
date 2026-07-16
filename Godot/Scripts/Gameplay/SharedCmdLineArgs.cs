using Soteo.Core.Enums;
using Soteo.Main.CampaignServer;

namespace Soteo.Main.Gameplay;

public static class SharedCmdLineArgs
{
    public static Side Side { get; }
    
    public static bool IsSingleplayer
    {
        get
        {
            return Side switch
            {
                Side.Client => ClientCmdLineArgs.IsSingleplayer,
                Side.ShardServer => ShardServerCmdLineArgs.IsSingleplayer,
                Side.CampaignServer => CampaignServerCmdLineArgs.IsSingleplayer,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
    
    static SharedCmdLineArgs()
    {
        bool isShardServer = OS.GetCmdlineArgs().Contains("--shard-server");
        bool isCampaignServer = OS.GetCmdlineArgs().Contains("--campaign-server");
        if (isShardServer && isCampaignServer)
            throw new ArgumentException("Both --shard-server and --campaign-server were specified");
        Side = isShardServer ? Side.ShardServer : isCampaignServer ? Side.CampaignServer : Side.Client;
    }
}
