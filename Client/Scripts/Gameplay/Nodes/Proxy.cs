using System.Reflection;

namespace Soteo.Gameplay.Nodes;

public sealed class Proxy : Node
{
    [Export] private string _replacementScriptFullName = "";
    
    public Type ReplacementType => Assembly.GetExecutingAssembly().GetType(_replacementScriptFullName);
    
    public override void _EnterTree()
    {
        throw new InvalidOperationException("ProxyNode should be replaced before entering the scene tree");
    }
}