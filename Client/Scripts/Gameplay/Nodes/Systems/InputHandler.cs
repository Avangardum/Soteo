using System.Collections.Immutable;
using Godot.Collections;
using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared;
using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Nodes.Systems;

public sealed class InputHandler : Node2D
{
    private IPacketSender _packetSender = null!;
    private IHud _hud = null!;
    private IEntityLocator _entityLocator = null!;
    private ICurrentUserIdRepository _currentUserIdRepo = null!;
    
    [Inject]
    public void Inject
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
    }

    public override void _Ready()
    {
        if (IsServer) QueueFree();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("select")) HandleSelect();
        if (e.IsActionPressed("interact")) HandleInteract();
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
        Unit? unit = GetUnitsUnderMouse().FirstOrDefault();
        if (unit == null) return;
        _hud.SelectedUnit = unit;
    }
    
    private void HandleInteract()
    {
        PlayerCharacter? user = _entityLocator.FindEntity<PlayerCharacter>(_currentUserIdRepo.UserId, out _);
        if (user == null) return;
        
        // Unit? targetUnit = GetUnitsUnderMouse().FirstOrDefault(it =>
        //     ValidateAbilityTargetUnit(character, AbilitySlot.Attack, it) == AbilityValidationResult.Ok);
        Unit? targetUnit = null;
        
        if (targetUnit != null)
        {
            // todo attack
        }
        else
        {
            _packetSender.SendReliable(new MovePacket { Position = GetGlobalMousePosition() }, Const.TestShardId);
        }
    }
    
    private void HandleUseAbility(AbilitySlot slot)
    {
        PlayerCharacter? user = _entityLocator.FindEntity<PlayerCharacter>(_currentUserIdRepo.UserId, out _);
        if (user == null || !user.AbilityStates.TryGetValue(slot, out IReadOnlyAbilityState? state)) return;
        
        bool canTargetUnit = state.Ability.TargetFlags.HasFlag(AbilityTargetFlags.Unit);
        bool canTargetPosition = state.Ability.TargetFlags.HasFlag(AbilityTargetFlags.Position);
        bool canTargetNothing = state.Ability.TargetFlags.HasFlag(AbilityTargetFlags.Untargeted);
        bool alt = Input.IsActionPressed("alt");
        
        IReadOnlyList<Unit> candidateTargetUnits = !canTargetUnit ? [] : alt ? [user] : GetUnitsUnderMouse();
        Unit? targetUnit = candidateTargetUnits
            .FirstOrDefault(it => ValidateAbilityTargetUnit(user, slot, it) == AbilityValidationResult.Ok);
        
        Vector2? targetPosition = canTargetPosition && targetUnit == null ? GetGlobalMousePosition() : null;
        
        if (!canTargetNothing && targetUnit == null && targetPosition == null) return;
        
        var command = new UseAbilityCommand(slot, targetPosition, targetUnit?.Id, null, null);
        _packetSender.SendReliable(new UseAbilityPacket { Command = command }, Const.TestShardId);
    }
    
    private AbilityValidationResult ValidateAbilityTargetUnit(Unit user, AbilitySlot slot, Unit target)
    {
        AbilityUseContext context = user.GetAbilityUseContext(new UseAbilityCommand(slot, TargetUnitId: target.Id));
        return user.AbilityStates[slot].Ability.Validate(context);
    }
    
    public override void _PhysicsProcess(float delta)
    {
        Input.ParseInputEvent(new InputEventAction { Action = "select", Pressed = true });
    }

    private IReadOnlyList<Unit> GetUnitsUnderMouse()
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
            .Select(it => it.GetParent() as Unit)
            .WhereNotNull()
            .OrderByDescending(it => it.ZIndex)
            .ThenByDescending(it => it.VisualPosition.y)
            .ToImmutableList();
    }
}