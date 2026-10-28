namespace Soteo.Main.Shared.Nodes.Autoloads;

public sealed partial class AppDomainValidator : Node // todo remove
{
    // This node detects the engine bug where current AppDomain changes and breaks everything
    // https://github.com/godotengine/godot/issues/118124

    public override void _PhysicsProcess(double delta)
    {
        if (AppDomain.CurrentDomain.FriendlyName != "GodotEngine.Domain.Scripts")
            throw new Exception("AppDomain switch detected, we are so cooked!");
    }
}
