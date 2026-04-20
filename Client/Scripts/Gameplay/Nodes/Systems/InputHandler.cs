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
        Unit? unit = GetUnitUnderMouse();
        if (unit == null) return;
        _hud.SelectedUnit = unit;
    }
    
    private void HandleInteract()
    {
        Unit? unit = GetUnitUnderMouse();
        if (unit != null)
        {
            GD.Print($"Right clicked on {unit}");
        }
        else
        {
            _packetSender.SendReliable(new MovePacket { Position = GetGlobalMousePosition() }, Const.TestShardId);
        }
    }
    
    private void HandleUseAbility(AbilitySlot slot)
    {
        PlayerCharacter? caster = _entityLocator.FindEntity<PlayerCharacter>(_currentUserIdRepo.UserId, out _);
        if (caster == null || !caster.AbilityStates.TryGetValue(slot, out IReadOnlyAbilityState? state)) return;
        
        bool canTargetUnit = state.Ability.TargetFlags.HasFlag(AbilityTargetFlags.Unit);
        bool canTargetPosition = state.Ability.TargetFlags.HasFlag(AbilityTargetFlags.Position);
        bool canTargetNothing = state.Ability.TargetFlags.HasFlag(AbilityTargetFlags.Untargeted);
        bool alt = Input.IsActionPressed("alt");
            
        Unit? targetUnit = !canTargetUnit ? null : alt ? caster : GetUnitUnderMouse();
        // todo check if target unit is valid, consider multiple units under mouse
        
        Vector2? targetPosition = canTargetPosition && targetUnit == null ? GetGlobalMousePosition() : null;
        
        if (!canTargetNothing && targetUnit == null && targetPosition == null) return;
        
        var command = new UseAbilityCommand(slot, targetPosition, targetUnit?.Id, null, null);
        _packetSender.SendReliable(new UseAbilityPacket { Command = command }, Const.TestShardId);
    }
    
    public override void _PhysicsProcess(float delta)
    {
        Input.ParseInputEvent(new InputEventAction { Action = "select", Pressed = true });
    }

    private Unit? GetUnitUnderMouse()
    {
        GdArray intersections = GetWorld2d().DirectSpaceState.IntersectPoint
        (
            GetGlobalMousePosition(),
            maxResults: 1,
            collisionLayer: (uint)CollisionLayer.ClickArea,
            collideWithBodies: false,
            collideWithAreas: true
        );
        if (intersections.Count == 0) return null;
        var clickArea = (Area2D)intersections.Cast<Dictionary>().Single()["collider"];
        return clickArea.GetParent() as Unit;
    }
}