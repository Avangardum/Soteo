namespace Soteo.Gameplay.Interfaces;

public interface IShardNode : IShard
{
    Node2D Node { get; }
    Node2D EntityRoot { get; }
}