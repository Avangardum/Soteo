using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Commands;

public sealed record UseAbilityCommand
(
    AbilitySlot Slot,
    Vector2? TargetPosition,
    Guid? TargetUnitId,
    Vector2? TargetDirection,
    Guid? TargetShardId
) : ICommand;