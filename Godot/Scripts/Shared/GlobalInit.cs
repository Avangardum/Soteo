using System.Globalization;
using Soteo.Core.Shared;

namespace Soteo.Shared;

public static class GlobalInit
{
    public static void Init()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        if ((int)ProjectSettings.GetSetting("physics/common/physics_fps") != Const.TicksPerSecond)
            throw new Exception("Godot physics FPS doesn't match Const.TicksPerSecond");
    }
}
