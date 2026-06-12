using Soteo.Core.Interfaces;

namespace Soteo.Core.Services;

public sealed class SideDetector : ISideDetector
{
    public static SideDetector Server => new(true);
    public static SideDetector Client => new(false);
    
    public bool IsServer { get; }
    
    public bool IsClient => !IsServer;
    
    public SideDetector(bool isServer)
    {
        IsServer = isServer;
    }
}
