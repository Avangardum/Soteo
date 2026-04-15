using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Interfaces;

public interface IHud
{
    Unit? SelectedUnit { get; set; }
}