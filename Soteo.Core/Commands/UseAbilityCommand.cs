using System.Numerics;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Commands;

public sealed record UseAbilityCommand
(
    AbilitySlot Slot,
    bool Repeat = false,
    Vector2? TargetPosition = null,
    Guid? TargetUnitId = null,
    Vector2? TargetDirection = null,
    Guid? TargetShardId = null
) : ICommand;