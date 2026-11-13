using Soteo.Core;

namespace Soteo.Main.Gameplay.Ui;

public sealed partial class Palette : Resource, IPalette
{
    [Export] public Color Neutral { get; private set; }
    [Export] public Color Empire { get; private set; }
    [Export] public Color Syndicate { get; private set; }

    public Color FactionColor(Faction faction)
    {
        return faction switch
        {
            Faction.Empire => Empire,
            Faction.Syndicate => Syndicate,
            _ => Neutral
        };
    }
}
