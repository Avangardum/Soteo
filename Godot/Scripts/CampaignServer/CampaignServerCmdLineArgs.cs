namespace Soteo.Main.CampaignServer;

public static class CampaignServerCmdLineArgs
{
    public static IReadOnlyList<Guid> ShardIds { get; }
    
    static CampaignServerCmdLineArgs()
    {
        string[] args = OS.GetCmdlineArgs();
        List<Guid> shardIds = [];
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].EndsWith(".tscn")) continue;
            
            if (args[i] == "--shard")
            {
                i++;
                if (i == args.Length) throw new ArgumentException("--shard should be followed by shard id");
                shardIds.Add(Guid.Parse(args[i]));
            }
        }
        
        ShardIds = shardIds;
    }
}
