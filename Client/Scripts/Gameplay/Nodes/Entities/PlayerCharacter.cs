namespace Soteo.Gameplay.Nodes.Entities;

public class PlayerCharacter : Unit
{
    private Label _label = null!;

    public override void _Ready()
    {
        base._Ready();
        _label = GetNode<Label>("Label");
    }

    public string DisplayName
    {
        get => _label.Text;
        set => _label.Text = value;
    }
}