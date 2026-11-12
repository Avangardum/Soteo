using System.Diagnostics.CodeAnalysis;
using Soteo.Core.Entities;
using Soteo.Core.Entities.Units;

namespace Soteo.Main.Gameplay.Interfaces;

public interface IHud
{
    IUnitPuppet? SelectedUnit { get; set; }

    [MemberNotNullWhen(true, nameof(SelectedUnit))]
    bool TrySelectCurrentUnit();
}
