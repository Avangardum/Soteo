using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Commands;

public sealed class CastCommand(AbilitySlot Slot, Guid? TargetId, Vector2? Target, Vector2? Direction, Guid? shardId) :
    ICommand;