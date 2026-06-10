using Soteo.Core.Enums;

namespace Soteo.Main.Gameplay.Interfaces;

public interface IPalette
{
    Color Neutral { get; }
    Color Empire { get; }
    Color Syndicate { get; }
    
    Color FactionColor(Faction faction);
}
