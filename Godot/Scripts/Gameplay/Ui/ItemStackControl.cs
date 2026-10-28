using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed partial class ItemStackControl : Control
{
    private const double DragThresholdPx = 10;

    private bool _isMouseHovering;
    private bool _isDragging;
    private GdVector2 _offset;
    private GdVector2? _mouseDownPosition;

    private readonly LateInit<Node> _originalParent = new();
    private readonly LateInit<Node> _dragAndDropLayer = new();
    private readonly LateInit<TextureRect> _textureRect = new();
    private readonly LateInit<Control> _countBackground = new();
    private readonly LateInit<Label> _countLabel = new();

    public bool IsInteractive { get; set; }

    public event Action SelectClicked = delegate { };
    public event Action DragStarted = delegate { };
    public event Action DragStopped = delegate { };

    public TextureRect TextureRect => _textureRect.Value;
    public Control CountBackground => _countBackground.Value;
    public Label CountLabel => _countLabel.Value;

    public override void _Ready()
    {
        _originalParent.Value = GetParent().Required;
        _dragAndDropLayer.Value = GetNode("/root/GameplayMain/Ui/DragAndDropLayer").Required;
        _textureRect.Value = GetNode<TextureRect>("TextureRect").Required;
        _countBackground.Value = GetNode<Control>("TextureRect/CountBackground").Required;
        _countLabel.Value = GetNode<Label>("TextureRect/CountBackground/CountLabel").Required;

        this.Connect("mouse_entered", () => _isMouseHovering = true);
        this.Connect("mouse_exited", () => _isMouseHovering = false);
    }

    public override void _Process(double delta)
    {
        if (!Visible)
        {
            _mouseDownPosition = null;
            _isMouseHovering = false;
            if (_isDragging) StopDrag();
            return;
        }

        if (IsInteractive && _isMouseHovering && Input.IsActionJustPressed(InputActions.Select))
        {
            _mouseDownPosition = GetGlobalMousePosition();
        }
        if (_mouseDownPosition != null && Input.IsActionJustReleased(InputActions.Select))
        {
            _mouseDownPosition = null;
            if (_isDragging)
                StopDrag();
            else
                SelectClicked();
        }

        if
        (
            !_isDragging &&
            _mouseDownPosition != null &&
            (_mouseDownPosition.Value - GetGlobalMousePosition()).LengthSquared() >= DragThresholdPx
        )
        {
            StartDrag();
        }

        if (_isDragging)
            GlobalPosition = GetGlobalMousePosition() + _offset;
    }

    private void StartDrag()
    {
        _isDragging = true;
        _offset = -GetLocalMousePosition();
        GdVector2 position = GlobalPosition;
        GetParent().RemoveChild(this);
        _dragAndDropLayer.Value.AddChild(this);
        GlobalPosition = position;
        DragStarted();
    }

    public void StopDrag()
    {
        _isDragging = false;
        _isMouseHovering = false;
        Position = GdVector2.Zero;
        GetParent().RemoveChild(this);
        _originalParent.Value.AddChild(this);
        DragStopped();
    }
}
