namespace Soteo.Main.CampaignServer;

public static class CampaignServerCmdLineArgs
{
    public static IReadOnlyList<Guid> ShardIds { get; }
    public static bool IsSingleplayer { get; }
    
    static CampaignServerCmdLineArgs()
    {
        string[] args = OS.GetCmdlineArgs();
        List<Guid> shardIds = [];
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].EndsWith(".tscn")) // todo custom argument for launching campaign server
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
