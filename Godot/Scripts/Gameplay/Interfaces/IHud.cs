using Soteo.Gameplay.Entities;

namespace Soteo.Gameplay.Interfaces;

public interface IHud
{
    Unit? SelectedUnit { get; set; }
}