namespace Soteo.Main.Gameplay;

public static class ClientCmdLineArgs
{
    public static bool IsSingleplayer { get; }
    public static string? Email { get; }
    public static bool NoScroll { get; }
    
    static ClientCmdLineArgs()
    {
        if (GameplayCmdLineArgs.IsServer)
            throw new InvalidOperationException($"{nameof(ClientCmdLineArgs)} should only be used on clients");
        
        string[] args = OS.GetCmdlineArgs();
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--singleplayer")
            {
                IsSingleplayer = true;
            }
            else if (args[i] == "--email")
            {
                if (Email != null)
                    throw new ArgumentException("Duplicate --email argument");
                i++;
                if (i == args.Length)
                    throw new ArgumentException("--email should be followed by a email address");
                Email = args[i];
            }
            else if (args[i] == "--no-scroll")
            {
                NoScroll = true;
            }
            else
            {
                throw new ArgumentException($"Unsupported command line argument {args[i]}");
            }
        }
    }
}
