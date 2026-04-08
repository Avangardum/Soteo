using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public class Unit : KinematicBody2D, IEntity
{
    [Export] private float _movementSpeed = 50;
    [Export] private float _rotationSpeedDeg = 360;
    
    private Line2D _azimuthLine = null!;
    
    protected Queue<ICommand> Commands { get; } = [];

    public Guid Id { get; set; }

    Node2D IEntity.Node => this;
    
    public float Azimuth
    {
        get;
        set
        {
            field = Mathf.PosMod(value, 360);
            _azimuthLine.RotationDegrees = field;
        }
    }
    
    public override void _Ready()
    {
        _azimuthLine = GetNode<Line2D>("AzimuthLine");
    }

    public override void _PhysicsProcess(float deltaTime)
    {
        float remainingDeltaTime = deltaTime;
        while (Commands.Count > 0 && remainingDeltaTime > 0)
        {
            switch (Commands.Peek())
            {
                case LookCommand command:
                    LookAtPosition(command.Position, ref remainingDeltaTime);
                    break;
                case MoveCommand command:
                    LookAtPosition(command.Position, ref remainingDeltaTime);
                    MoveToPosition(command.Position, ref remainingDeltaTime);
                    break;
            }
        }
    }
    
    private void LookAtPosition(Vector2 position, ref float remainingDeltaTime)
    {
        LookInDirection(position - Position, ref remainingDeltaTime);
    }
    
    private void LookInDirection(Vector2 direction, ref float remainingDeltaTime)
    {
        LookAtAzimuth(SoteoMath.DirectionToAzimuth(direction), ref remainingDeltaTime);
    }
    
    private void LookAtAzimuth(float azimuth, ref float remainingDeltaTime)
    {
        if (remainingDeltaTime == 0) return;
        
        float desiredDeltaAzimuth = azimuth - Azimuth;
        if (desiredDeltaAzimuth > 180) desiredDeltaAzimuth -= 360;
        if (desiredDeltaAzimuth < -180) desiredDeltaAzimuth += 360;
        
        float timeToComplete = Mathf.Abs(desiredDeltaAzimuth) / _rotationSpeedDeg;
        if (timeToComplete <= remainingDeltaTime)
        {
            Azimuth += desiredDeltaAzimuth;
            remainingDeltaTime -= timeToComplete;
            if (Commands.PeekOrDefault() is LookCommand) Commands.Dequeue();
        }
        else
        {
            Azimuth += Mathf.Sign(desiredDeltaAzimuth) * remainingDeltaTime * _rotationSpeedDeg;
            remainingDeltaTime = 0;
        }
    }
    
    private void MoveToPosition(Vector2 position, ref float remainingDeltaTime)
    {
        if (remainingDeltaTime == 0) return;
        
        Vector2 desiredMovement = position - Position;
        float desiredMovementLength = desiredMovement.Length();
        float timeToComplete = desiredMovementLength / _movementSpeed;
        if (timeToComplete <= remainingDeltaTime)
        {
            MoveAndCollide(desiredMovement);
            remainingDeltaTime -= timeToComplete;
            if (Commands.PeekOrDefault() is MoveCommand) Commands.Dequeue();
        }
        else
        {
            MoveAndCollide(desiredMovement / desiredMovementLength * _movementSpeed * remainingDeltaTime);
            remainingDeltaTime = 0;
        }
    }

    public void SetCommand(ICommand command)
    {
        Commands.Clear();
        Commands.Enqueue(command);
    }
     
    public void CancelCommands()
    {
        Commands.Clear();
    }
}