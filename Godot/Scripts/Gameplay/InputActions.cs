using Soteo.Core.Enums;

namespace Soteo.Main.Gameplay;

public static class InputActions
{
    public const string Select = "select";
    public const string Interact = "interact";
    public const string ZoomIn = "zoom_in";
    public const string ZoomOut = "zoom_out";
    public const string DragCamera = "drag_camera";
    public const string Alt = "alt";
    public const string DebugScreen = "debug_screen";
    public const string Stop = "stop";

    public static string UseAbility(AbilitySlot slot) =>
        "use_ability_" + slot.ToString().ToLower();
}
