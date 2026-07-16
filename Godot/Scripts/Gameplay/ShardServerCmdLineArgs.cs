using Soteo.Core.Enums;

namespace Soteo.Main.Gameplay;

public static class ShardServerCmdLineArgs
{
    public static Guid ShardId { get; }
    public static bool IsSingleplayer { get; }
    
    static ShardServerCmdLineArgs()
    {
        if (SharedCmdLineArgs.Side != Side.ShardServer)
            throw new InvalidOperationException("This class is for the shard server only");
        
        string[] args = OS.GetCmdlineArgs();
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--shard-server")
            {
                if (ShardId != Guid.Empty)
                    throw new ArgumentException("Duplicate --shard-server argument");
                i++;
                if (i == args.Length)
                    throw new ArgumentException("--shard-server should be followed by the shard id");
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
