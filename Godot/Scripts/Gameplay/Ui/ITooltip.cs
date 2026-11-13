namespace Soteo.Main.Gameplay.Interfaces;

public interface ITooltip
{
    void Show(Vector2 position, string header, string body);
    void Hide();
}
