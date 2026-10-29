using Soteo.Core.Enums;
using Soteo.Main.Gameplay.Interfaces;

namespace Soteo.Main.Gameplay.Resources;

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
