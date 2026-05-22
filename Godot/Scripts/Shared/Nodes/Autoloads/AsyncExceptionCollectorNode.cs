namespace Soteo.Shared.Nodes.Autoloads;

public sealed class AsyncExceptionCollectorNode : Node
{
    public override void _Process(float delta)
    {
        AsyncExceptionCollector.Process();
    }
}