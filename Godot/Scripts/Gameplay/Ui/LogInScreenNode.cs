namespace Soteo.Main.Gameplay.Ui;

public sealed partial class LogInScreenNode : Control
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/LogIn.tscn");

    private LogInScreenNode() { }

    public static LogInScreenNode Instance() => Scene.Instantiate<LogInScreenNode>(); // todo rename
}
