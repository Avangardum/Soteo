using Soteo.Util;

namespace Soteo.Main.Shared.Nodes.Autoloads;

public sealed class AsyncExceptionCollectorNode : Node
{
    public override void _Ready()
    {
        PauseMode = PauseModeEnum.Process;
    }

    public override void _Process(float delta)
    {
        AsyncExceptionCollector.Process();
    }
}
