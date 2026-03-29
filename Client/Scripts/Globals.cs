using System.Linq;

namespace Soteo.Client;

public static class Globals
{
    public static readonly bool IsServer = OS.GetCmdlineArgs().Contains("--server");
    public static readonly Guid MasterServerId = Guid.Parse("00000000-0000-0000-0000-00000000b055");
}