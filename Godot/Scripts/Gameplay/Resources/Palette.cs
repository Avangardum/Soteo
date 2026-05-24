using Soteo.Core.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Resources;

public sealed class Palette : Resource, IPalette
{
    [field: Export] public Color Neutral { get; }
    [field: Export] public Color Empire { get; }
    [field: Export] public Color Syndicate { get; }
    
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