using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Commands;

public sealed class CastAbilityCommand
(
    AbilitySlot Slot,
    Guid? TargetUnitId,
    Vector2? TargetPoint,
    Vector2? TargetDirection,
    Guid? TargetShardId
) : ICommand;