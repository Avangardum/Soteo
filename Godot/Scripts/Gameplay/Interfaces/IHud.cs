using Soteo.Gameplay.Entities;

namespace Soteo.Gameplay.Interfaces;

public interface IHud
{
    UnitPuppet? SelectedUnit { get; set; }
}