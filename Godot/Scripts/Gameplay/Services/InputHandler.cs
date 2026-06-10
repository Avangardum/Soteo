using System.Collections.Immutable;
using Godot.Collections;
using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Extensions;
using Soteo.Util;
using Soteo.Util.Extensions;

namespace Soteo.Gameplay.Services;

public sealed class InputHandler : Node2D
{
    private readonly IFromGameplayPacketSender _packetSender;
    private readonly IHud _hud;
    private readonly IEntityLocator _entityLocator;
    private readonly ICurrentCharacterIdRepository _currentCharIdRepo;
    
    public InputHandler
    (
        IFromGameplayPacketSender packetSender,
        IHud hud,
        IEntityLocator entityLocator,
        ICurrentCharacterIdRepository currentCharIdRepo
    )
    {
        _packetSender = packetSender;
        _hud = hud;
        _entityLocator = entityLocator;
        _currentCharIdRepo = currentCharIdRepo;
        
        Name = nameof(InputHandler);
        PauseMode = PauseModeEnum.Process;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("select"))
            HandleSelect();
        if (e.IsActionPressed("interact"))
            HandleInteract();
        if (e.IsActionPressed("stop"))
            HandleStop();
        
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
        if (_currentCharIdRepo.Value == null) return;
        _hud.TrySelectCurrentUnit();
        if (!_entityLocator.TryFindEntity(_currentCharIdRepo.Required, out UnitPuppet? user, out Guid? shardId)) return;
        
        UnitPuppet? targetUnit = GetUnitsUnderMouse()
            .FirstOrDefault(it => ValidateAbility(user, AbilitySlot.Attack, it) == AbilityValidationResult.Ok);
        
        if (targetUnit != null)
        {
            var command = new UseAbilityCommand(Slot: AbilitySlot.Attack, Repeat: true, TargetUnitId: targetUnit.Id);
            _packetSender.SendReliable
            (
                new UseAbilityPacket { UnitId = _currentCharIdRepo.Required, Command = command },
                shardId.Value
            );
        }
        else
        {
            _packetSender.SendReliable
            (
                new MovePacket
                {
                    UnitId = _currentCharIdRepo.Required,
                    Command = new MoveCommand(GetGlobalMousePosition().ToSys()),
                },
                shardId.Value
            );
        }
    }
    
    private void HandleStop()
    {
        if (_currentCharIdRepo.Value == null) return;
        _hud.TrySelectCurrentUnit();
        if (!_entityLocator.TryFindEntity(_currentCharIdRepo.Required, out UnitPuppet? user, out Guid? shardId)) return;
        _packetSender.SendReliable
        (
            new StopPacket { UnitId = _currentCharIdRepo.Required, Command = new StopCommand() },
            shardId.Value
        );
    }

    private void HandleUseAbility(AbilitySlot slot)
    {
        if (_currentCharIdRepo.Value == null) return;
        _hud.TrySelectCurrentUnit();
        if (!_entityLocator.TryFindEntity(_currentCharIdRepo.Required, out UnitPuppet? user, out Guid? shardId)) return;
        if (!user.AbilitySlotStates.TryGetValue(slot, out AbilitySlotState? state)) return;

        UnitPuppet? targetUnit = null;
        Vector2? targetPosition = null;
        
        bool alt = Input.IsActionPressed("alt");
        bool forceNoTarget = alt && state.Ability.Targeting.HasFlag(CanTarget.Nothing);
        
        if (!forceNoTarget)
        {
            IReadOnlyList<UnitPuppet> candidateTargetUnits = alt ? [user] : GetUnitsUnderMouse();
            targetUnit = candidateTargetUnits
                .FirstOrDefault(it => ValidateAbility(user, slot, it) == AbilityValidationResult.Ok);
            
            bool canTargetPosition = state.Ability.Targeting.HasFlag(CanTarget.Position);
            targetPosition = canTargetPosition && targetUnit == null ? GetGlobalMousePosition().ToSys() : null;
        }

        _packetSender.SendReliable
        (
            new UseAbilityPacket
            {
                UnitId = _currentCharIdRepo.Required,
                Command = new UseAbilityCommand(slot, Repeat: false, targetPosition, targetUnit?.Id) 
            },
            shardId.Value
        );
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
            if (!_entityLocator.TryFindEntity(command.TargetUnitId.Value, out UnitPuppet? targetUnit, out _))
                return AbilityValidationResult.InvalidTarget;
            if (targetUnit.IsAlliedTo(user) && !state.Ability.Targeting.HasFlag(CanTarget.Ally))
                return AbilityValidationResult.InvalidTarget;
            if (!targetUnit.IsAlliedTo(user) && !state.Ability.Targeting.HasFlag(CanTarget.Enemy))
                return AbilityValidationResult.InvalidTarget;
        }
        
        // todo validate character / building
        
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
