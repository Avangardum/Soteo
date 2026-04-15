using Godot.Collections;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Nodes.Systems;

public sealed class InputHandler : Node2D
{
    private IPacketSender _packetSender = null!;
    private IHud _hud = null!;
    
    [Inject]
    public void Inject(IPacketSender packetSender, IHud hud)
    {
        _packetSender = packetSender;
        _hud = hud;
    }

    public override void _Ready()
    {
        if (IsServer) QueueFree();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("select")) HandleSelect();
        if (e.IsActionPressed("interact")) HandleInteract();
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