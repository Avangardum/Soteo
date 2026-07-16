namespace Soteo.Main.Gameplay;

public static class ShardServerCmdLineArgs
{
    public static Guid ShardId { get; }
    public static bool IsSingleplayer { get; }
    
    static ShardServerCmdLineArgs()
    {
        if (!GameplayCmdLineArgs.IsServer)
        {
            var message = $"{nameof(ShardServerCmdLineArgs)} should only be used on shard servers";
            throw new InvalidOperationException(message);
        }
        
        string[] args = OS.GetCmdlineArgs();
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--server")
            {
                if (ShardId != Guid.Empty)
                    throw new ArgumentException("Duplicate --server argument");
                i++;
                if (i == args.Length)
                    throw new ArgumentException("--server should be followed by the shard id");
                ShardId = Guid.Parse(args[i]);
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
    }
}
