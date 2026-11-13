using Soteo.Util;

namespace Soteo.Main.Shared.Nodes.Autoloads;

public sealed partial class AsyncExceptionCollectorNode : Node
{
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        AsyncExceptionCollector.Process();
    }
}
