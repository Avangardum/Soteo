namespace Soteo.Client.Nodes.Systems;

public sealed class AppDomainValidator : Node
{
    // This node detects the engine bug where current AppDomain changes and breaks everything
    // https://github.com/godotengine/godot/issues/118124
    
    public override void _PhysicsProcess(float delta)
    {
        if (AppDomain.CurrentDomain.FriendlyName != "GodotEngine.Domain.Scripts")
            throw new InvalidOperationException("AppDomain switch detected, we are so cooked!");
    }
}