using System.Collections.Immutable;
using Godot.Collections;
using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Services;

public sealed class InputHandler : Node2D
{
    private readonly IPacketSender _packetSender;
    private readonly IHud _hud;
    private readonly IEntityLocator _entityLocator;
    private readonly ICurrentUserIdRepository _currentUserIdRepo;
    
    public InputHandler
    (
        IPacketSender packetSender,
        IHud hud,
        IEntityLocator entityLocator,
        ICurrentUserIdRepository currentUserIdRepo
    )
    {
        _packetSender = packetSender;
        _hud = hud;
        _entityLocator = entityLocator;
        _currentUserIdRepo = currentUserIdRepo;
        
        Name = nameof(InputHandler);
    }

    public override void _Ready()
    {
        if (IsServer) QueueFree();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("select"))
            HandleSelect();
        if (e.IsActionPressed("interact"))
            HandleInteract();
        if (e.IsActionPressed("stop"))
            _packetSender.SendReliable(new StopPacket(), Const.TestShardId);
        
        foreach (var slot in Enum.GetValues<AbilitySlot>().Distinct())
        {
            var action = "use_ability_" + slot.ToString().ToLower();
            if (InputMap.HasAction(action) && e.IsActionReleased(action))
            {
                HandleUseAbility(slot);
            }
        }
    }
    
    private void HandleSelect()
    {
        UnitPuppet? unit = GetUnitsUnderMouse().FirstOrDefault();
        if (unit != null)
            _hud.SelectedUnit = unit;
    }
    
    private void HandleInteract()
    {
        UnitPuppet? user = _entityLocator.FindEntity<UnitPuppet>(_currentUserIdRepo.UserId, out _);
        if (user == null) return;
        
        UnitPuppet? targetUnit = GetUnitsUnderMouse()
            .FirstOrDefault(it => ValidateAbility(user, AbilitySlot.Attack, it) == AbilityValidationResult.Ok);
        
        if (targetUnit != null)
        {
            var command  = new UseAbilityCommand(Slot: AbilitySlot.Attack, Repeat: true, TargetUnitId: targetUnit.Id);
            _packetSender.SendReliable(new UseAbilityPacket { Command = command }, Const.TestShardId);
        }
        else
        {
            _packetSender.SendReliable
            (
                new MovePacket { Position = GetGlobalMousePosition().ToSys() },
                Const.TestShardId
            );
        }
    }

    private void HandleUseAbility(AbilitySlot slot)
    {
        UnitPuppet? user = _entityLocator.FindEntity<UnitPuppet>(_currentUserIdRepo.UserId, out _);
        if (user == null || !user.AbilitySlotStates.TryGetValue(slot, out AbilitySlotState? state)) return;

        IReadOnlyList<UnitPuppet> candidateTargetUnits =
            Input.IsActionPressed("alt") ? [user] : GetUnitsUnderMouse();
        UnitPuppet? targetUnit = candidateTargetUnits
            .FirstOrDefault(it => ValidateAbility(user, slot, it) == AbilityValidationResult.Ok);
        
        bool canTargetPosition = state.Ability.Targeting.HasFlag(CanTarget.Position);
        Vector2? targetPosition = canTargetPosition && targetUnit == null ? GetGlobalMousePosition().ToSys() : null;
        var command = new UseAbilityCommand(slot, Repeat: false, targetPosition, targetUnit?.Id);
        if (targetUnit == null && ValidateAbility(user, command) != AbilityValidationResult.Ok) return;
        _packetSender.SendReliable(new UseAbilityPacket { Command = command }, Const.TestShardId);
    }

    private AbilityValidationResult ValidateAbility(UnitPuppet user, AbilitySlot slot, UnitPuppet targetUnit) =>
        ValidateAbility(user, new UseAbilityCommand(slot, TargetUnitId: targetUnit.Id));
    
    private AbilityValidationResult ValidateAbility
    (
        UnitPuppet user,
        UseAbilityCommand command
    )
    {
        // This method only validates target to select a valid target in a crowd
        
        AbilitySlotState state = user.AbilitySlotStates[command.Slot];
        
        if (command.TargetUnitId != null)
        {
            UnitPuppet? targetUnit = _entityLocator.FindEntity<UnitPuppet>(command.TargetUnitId.Value, out _);
            if (targetUnit == null)
                return AbilityValidationResult.InvalidTarget;
            if (targetUnit.IsAlliedTo(user) && !state.Ability.Targeting.HasFlag(CanTarget.Ally))
                return AbilityValidationResult.InvalidTarget;
            if (!targetUnit.IsAlliedTo(user) && !state.Ability.Targeting.HasFlag(CanTarget.Enemy))
                return AbilityValidationResult.InvalidTarget;
        }
        
        return AbilityValidationResult.Ok;
    }

    private IReadOnlyList<UnitPuppet> GetUnitsUnderMouse()
    {
        GdArray intersections = GetWorld2d().DirectSpaceState.IntersectPoint
        (
            GetGlobalMousePosition(),
            maxResults: 32,
            collisionLayer: (uint)CollisionLayer.ClickArea,
            collideWithBodies: false,
            collideWithAreas: true
        );
        return intersections
            .Cast<Dictionary>()
            .Select(it => (Area2D)it["collider"])
            .Select(it => it.GetParent() as UnitPuppetNode)
            .WhereNotNull()
            .OrderByDescending(it => it.ZIndex)
            .ThenByDescending(it => it.Position.Y)
            .Select(it => it.UnitPuppet)
            .WhereNotNull()
            .ToImmutableList();
    }
}