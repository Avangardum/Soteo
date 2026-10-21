using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed class ItemStackControl : Control
{
    private bool _isMouseHovering;
    private bool _isDragging;
    private GdVector2 _offset;

    private readonly LateInit<Node> _originalParent = new();
    private readonly LateInit<Node> _dragAndDropLayer = new();
    private readonly LateInit<TextureRect> _textureRect = new();
    private readonly LateInit<Control> _countBackground = new();
    private readonly LateInit<Label> _countLabel = new();

    public bool IsDraggable { get; set; }

    public event Action DragStarted = delegate { };
    public event Action DragStopped = delegate { };

    public TextureRect TextureRect => _textureRect.Value;
    public Control CountBackground => _countBackground.Value;
    public Label CountLabel => _countLabel.Value;

    public override void _Ready()
    {
        _originalParent.Value = GetParent().Required;
        _dragAndDropLayer.Value = GetNode("/root/GameplayMain/Ui/DragAndDropLayer").Required;
        _textureRect.Value = GetNode<TextureRect>("TextureRect");
        _countBackground.Value = GetNode<Control>("TextureRect/CountBackground");
        _countLabel.Value = GetNode<Label>("TextureRect/CountBackground/CountLabel");

        this.Connect("mouse_entered", () => _isMouseHovering = true);
        this.Connect("mouse_exited", () => _isMouseHovering = false);
    }

    public override void _Process(float delta)
    {
        if (IsDraggable && _isMouseHovering && Input.IsActionJustPressed(InputActions.Select))
            StartDrag();
        else if (_isDragging && Input.IsActionJustReleased(InputActions.Select))
            StopDrag();

        if (_isDragging)
            RectGlobalPosition = GetGlobalMousePosition() + _offset;
    }

    private void StartDrag()
    {
        _isDragging = true;
        _offset = -GetLocalMousePosition();
        GdVector2 position = RectGlobalPosition;
        GetParent().RemoveChild(this);
        _dragAndDropLayer.Value.AddChild(this);
        RectGlobalPosition = position;
        DragStarted();
    }

    public void StopDrag()
    {
        _isDragging = false;
        _isMouseHovering = false;
        RectPosition = GdVector2.Zero;
        GetParent().RemoveChild(this);
        _originalParent.Value.AddChild(this);
        DragStopped();
    }
}
