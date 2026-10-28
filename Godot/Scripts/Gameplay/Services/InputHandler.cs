using System.Collections.Immutable;
using Godot.Collections;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Dto.Packets.Command;
using Soteo.Core.Entities;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;
using Soteo.Main.Gameplay.EntityNodes;
using Soteo.Main.Gameplay.Enums;
using Soteo.Main.Gameplay.Interfaces;

namespace Soteo.Main.Gameplay.Services;

public sealed partial class InputHandler : Node2D
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
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed(InputActions.Select))
            HandleSelect();
        if (e.IsActionPressed(InputActions.Interact))
            HandleInteract();
        if (e.IsActionPressed(InputActions.Stop))
            HandleStop();

        foreach (var slot in Enum.GetValues<AbilitySlot>().Distinct())
        {
            string action = InputActions.UseAbility(slot);
            if (InputMap.HasAction(action) && e.IsActionReleased(action))
            {
                HandleUseAbility(slot);
            }
        }
    }

    private void HandleSelect()
    {
        IUnitPuppet? unit = GetUnitsUnderMouse().FirstOrDefault();
        if (unit != null)
            _hud.SelectedUnit = unit;
    }

    private void HandleInteract()
    {
        if (_currentCharIdRepo.Id == null) return;
        _hud.TrySelectCurrentUnit();
        if (!_entityLocator.TryFindEntity(_currentCharIdRepo.Id.Required, out IUnitPuppet? user, out Guid? shardId))
            return;

        IUnitPuppet? targetUnit = GetUnitsUnderMouse().FirstOrDefault
        (
            it => ValidateAbility(user: user, slot: AbilitySlot.Attack, targetUnit: it, alt: false) ==
                AbilityValidationResult.Ok
        );

        if (targetUnit != null)
        {
            var command = new UseAbilityCommand
            {
                Slot = AbilitySlot.Attack,
                Repeat = true,
                TargetUnitId = targetUnit.Id
            };
            _packetSender.SendReliable
            (
                new UseAbilityPacket { UnitId = _currentCharIdRepo.Id.Required, Command = command },
                shardId.Value
            );
        }
        else
        {
            _packetSender.SendReliable
            (
                new MovePacket
                {
                    UnitId = _currentCharIdRepo.Id.Required,
                    Command = new MoveCommand(GetGlobalMousePosition().ToSys() / Const.PixelsInMeter)
                },
                shardId.Value
            );
        }
    }

    private void HandleStop()
    {
        if (_currentCharIdRepo.Id == null) return;
        _hud.TrySelectCurrentUnit();
        if (!_entityLocator.TryFindEntity(_currentCharIdRepo.Id.Required, out IUnitPuppet? user, out Guid? shardId))
            return;
        _packetSender.SendReliable
        (
            new StopPacket { UnitId = _currentCharIdRepo.Id.Required, Command = new StopCommand() },
            shardId.Value
        );
    }

    private void HandleUseAbility(AbilitySlot slot)
    {
        if (_currentCharIdRepo.Id == null) return;
        _hud.TrySelectCurrentUnit();
        if (!_entityLocator.TryFindEntity(_currentCharIdRepo.Id.Required, out IUnitPuppet? user, out Guid? shardId))
            return;
        if (!user.AbilitySlotStates.TryGetValue(slot, out AbilitySlotState? state)) return;

        bool alt = Input.IsActionPressed(InputActions.Alt);
        Targeting targeting = alt ? state.Ability.AltTargeting : state.Ability.Targeting;

        IUnitPuppet? targetUnit = GetUnitsUnderMouse()
            .FirstOrDefault(it => ValidateAbility(user, slot, it, alt) == AbilityValidationResult.Ok);

        Vector2? targetPosition = targeting.HasFlag(Targeting.Position) && targetUnit == null ?
            GetGlobalMousePosition().ToSys() / Const.PixelsInMeter :
            null;

        _packetSender.SendReliable
        (
            new UseAbilityPacket
            {
                UnitId = _currentCharIdRepo.Id.Required,
                Command = new UseAbilityCommand
                {
                    Slot = slot,
                    Repeat = false,
                    Alt = alt,
                    TargetPosition = targetPosition,
                    TargetUnitId = targetUnit?.Id
                }
            },
            shardId.Value
        );
    }

    private AbilityValidationResult ValidateAbility
    (
        IUnitPuppet user,
        AbilitySlot slot,
        IUnitPuppet targetUnit,
        bool alt
    )
    {
        return ValidateAbility(user, new UseAbilityCommand { Slot = slot, TargetUnitId = targetUnit.Id, Alt = alt });
    }

    private AbilityValidationResult ValidateAbility
    (
        IUnitPuppet user,
        UseAbilityCommand command
    )
    {
        // This method only validates target to select a valid target in a crowd

        AbilitySlotState state = user.AbilitySlotStates[command.Slot];
        Targeting targeting = command.Alt ? state.Ability.AltTargeting : state.Ability.Targeting;

        if (command.TargetUnitId != null)
        {
            if (!_entityLocator.TryFindEntity(command.TargetUnitId.Value, out IUnitPuppet? targetUnit, out _))
                return AbilityValidationResult.InvalidTarget;
            if (targetUnit.IsAlliedTo(user) && !targeting.HasFlag(Targeting.Ally))
                return AbilityValidationResult.InvalidTarget;
            if (!targetUnit.IsAlliedTo(user) && !targeting.HasFlag(Targeting.Enemy))
                return AbilityValidationResult.InvalidTarget;
        }

        return AbilityValidationResult.Ok;
    }

    private IReadOnlyList<IUnitPuppet> GetUnitsUnderMouse()
    {
        Array<Dictionary> intersections = GetWorld2D().DirectSpaceState.IntersectPoint
        (
            new PhysicsPointQueryParameters2D
            {
                Position = GetGlobalMousePosition(),
                CollisionMask = (uint)CollisionLayer.ClickArea,
                CollideWithBodies = false,
                CollideWithAreas = true
            },
            maxResults: 32
        );
        return intersections
            .Select(it => (Area2D)it["collider"])
            .Select(it => it.GetParent() as UnitPuppetNode)
            .WhereNotNull()
            .OrderByDescending(it => it.ZIndex)
            .ThenByDescending(it => it.PositionMeters.Y)
            .Select(it => it.UnitPuppet)
            .WhereNotNull()
            .ToImmutableList();
    }
}
