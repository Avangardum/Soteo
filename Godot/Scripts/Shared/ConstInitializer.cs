using System.Globalization;
using Soteo.Core.Shared;
using Soteo.Gameplay;
using Soteo.Util;

namespace Soteo.Shared;

public static class ConstInitializer
{
    public static void Init()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        
        Const.IsServer.Value = OS.GetCmdlineArgs().Contains("--server") || Main.EditorIsServer;
        
        int physicsFps = (int)ProjectSettings.GetSetting("physics/common/physics_fps");
        if (physicsFps != Const.TicksPerSecond)
        {
            throw new Exception
            (
                $"Const.TicksPerSecond = {Const.TicksPerSecond}, physics/common/physics_fps = {physicsFps}"
            );
        }
    }
}
