using System.Globalization;
using Soteo.Core;
using Soteo.Core.StaticHelpers;

namespace Soteo.Main.Shared;

public static class GlobalInit
{
    public static void Init()
    {
        TempLogger.Log = GD.Print;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        if ((int)ProjectSettings.GetSetting("physics/common/physics_ticks_per_second") != Const.TicksPerSecond)
            throw new Exception("Godot physics FPS doesn't match Const.TicksPerSecond");
    }
}
