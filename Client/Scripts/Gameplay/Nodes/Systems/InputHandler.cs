using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Nodes.Systems;

public sealed class InputHandler : Node2D
{
    private IPacketSender _packetSender = null!;
    
    public void Inject(IPacketSender packetSender)
    {
        _packetSender = packetSender;
    }
    
    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("interact")) HandleInteract(); 
    }
    
    private void HandleInteract()
    {
        Vector2 position = GetGlobalMousePosition();
        GdArray intersections = GetWorld2d().DirectSpaceState.IntersectPoint
        (
            position,
            maxResults: 1,
            collisionLayer: (uint)CollisionLayer.ClickArea,
            collideWithBodies: false,
            collideWithAreas: true
        );
        
        if (intersections.Count != 0)
        {
            GD.Print($"Right clicked on {intersections[0]}");
        }
        else
        {
            _packetSender.SendReliable(new MovePacket { Position = position }, Const.TestShardId);
        }
    }
}