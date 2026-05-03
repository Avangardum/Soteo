using Soteo.Gameplay.Enums;

namespace Soteo.Gameplay.Interfaces;

public interface IPalette
{
    Color Neutral { get; }
    Color Empire { get; }
    Color Syndicate { get; }
    
    Color FactionColor(Faction faction);
}