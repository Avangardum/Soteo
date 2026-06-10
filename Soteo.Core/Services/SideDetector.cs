using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Services;

public sealed class SideDetector : ISideDetector
{
    public bool IsServer { get; }
    
    public bool IsClient => !IsServer;
    
    public SideDetector(bool isServer)
    {
        IsServer = isServer;
    }
    
    public static SideDetector Server => new(true);
    public static SideDetector Client => new(false);
}
