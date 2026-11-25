namespace Soteo.Main.Gameplay.Ui;

public sealed partial class Tooltip : Control, ITooltip
{
    private float _bodyDefaultMinWidth;

    public Label HeaderLabel { get; private set; } = null!;
    public HSeparator Separator { get; private set; } = null!;
    public RichTextLabel BodyLabel { get; private set; } = null!;

    public override void _Ready()
    {
        HeaderLabel = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Header");
        Separator = GetNode<HSeparator>("PanelContainer/MarginContainer/VBoxContainer/HSeparator");
        BodyLabel = GetNode<RichTextLabel>("PanelContainer/MarginContainer/VBoxContainer/Body");

        _bodyDefaultMinWidth = BodyLabel.CustomMinimumSize.X;
    }

    public void Show(Vector2 position, string header, string body)
    {
        Visible = true;
        GlobalPosition = position.ToGd();
        HeaderLabel.Text = header;
        BodyLabel.Text = body;
        HeaderLabel.Visible = header != "";
        BodyLabel.Visible = body != "";
        Separator.Visible = header != "" && body != "";
        UpdateBodyMinWidth();
    }

    private void UpdateBodyMinWidth()
    {
        BodyLabel.CustomMinimumSize = new GdVector2(_bodyDefaultMinWidth, 0);
        BodyLabel.Size = BodyLabel.Size with { X = _bodyDefaultMinWidth };

        string text = BodyLabel.Text;
        BodyLabel.Text = "Lorem ipsum";
        int lineHeight = BodyLabel.GetContentHeight();
        BodyLabel.Text = text;

        const int step = 10;
        while (BodyLabel.GetContentHeight() == lineHeight && BodyLabel.CustomMinimumSize.X > step)
        {
            BodyLabel.CustomMinimumSize -= new GdVector2(step, 0);
            BodyLabel.Size = BodyLabel.Size with { X = BodyLabel.CustomMinimumSize.X };
        }
        BodyLabel.CustomMinimumSize += new GdVector2(step, 0);
    }

    public new void Hide() => Visible = false;
}
