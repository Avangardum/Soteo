namespace Soteo.Gameplay.Ui;

public sealed class LogInScreenNode : Control
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/LogIn.tscn");
    
    private LogInScreenNode() { }
    
    public static LogInScreenNode Instance() => Scene.Instance<LogInScreenNode>();
}
