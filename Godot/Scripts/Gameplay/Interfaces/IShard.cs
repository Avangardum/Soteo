namespace Soteo.Gameplay.Interfaces;

public interface IShard
{
    Guid Id { get; }
    Node2D Node { get; }
    Node2D EntityRoot { get; }
}