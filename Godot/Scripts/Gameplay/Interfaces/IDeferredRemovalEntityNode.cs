using Soteo.Core.Interfaces;

namespace Soteo.Main.Gameplay.Interfaces;

/// <summary>
/// Normally entity nodes are removed immediately when their entity is removed.
/// Nodes implementing this interface stay in the scene tree for some time before signaling that they can be removed.
/// </summary>
public interface IDeferredRemovalEntityNode : IEntityNode
{
    Task WaitUntilCanRemoveAsync();
}
