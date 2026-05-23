using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Commands;

public sealed record UseAbilityCommand
(
    AbilitySlot Slot,
    bool Repeat = false,
    GdVector2? TargetPosition = null,
    Guid? TargetUnitId = null,
    GdVector2? TargetDirection = null,
    Guid? TargetShardId = null
) : ICommand;