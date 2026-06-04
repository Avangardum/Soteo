using System.Diagnostics.CodeAnalysis;
using Soteo.Core.Gameplay.Entities;

namespace Soteo.Gameplay.Interfaces;

public interface IHud
{
    UnitPuppet? SelectedUnit { get; set; }
    
    [MemberNotNullWhen(true, nameof(SelectedUnit))]
    bool TrySelectCurrentUnit();
}
