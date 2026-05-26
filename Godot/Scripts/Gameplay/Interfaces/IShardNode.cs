using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Gameplay.Interfaces;

public interface IShardNode : IShard // todo use the class directly instead of this interface
{
    Node2D Node { get; }
    Node2D EntityRoot { get; }
}