using Soteo.Core;

namespace Soteo.Main.Gameplay.Ui;

public interface IPalette
{
    Color Neutral { get; }
    Color Empire { get; }
    Color Syndicate { get; }

    Color FactionColor(Faction faction);
}
