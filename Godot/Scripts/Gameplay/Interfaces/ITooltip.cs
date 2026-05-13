namespace Soteo.Gameplay.Interfaces;

public interface ITooltip
{
    bool Visible { get; set; }
    Vector2 RectGlobalPosition { get; set; }
    string Header { get; set; }
    string Body { get; set; }
}