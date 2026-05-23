namespace Soteo.Gameplay.Interfaces;

public interface ITooltip
{
    void Show(GdVector2 position, string header, string body);
    void Hide();
}