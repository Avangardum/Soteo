using System.Numerics;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Commands;

public sealed record UseAbilityCommand : ICommand
{
    public required AbilitySlot Slot { get; init; }
    public bool Repeat { get; init; }
    public bool Alt { get; init; }
    public Vector2? TargetPosition { get; init; }
    public Guid? TargetUnitId { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }
}