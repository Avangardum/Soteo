namespace Soteo.Main.Gameplay.Ui;

public sealed partial class ItemStackControl : Control
{
    private const double DragThresholdPx = 10;

    private bool _isMouseHovering;
    private bool _isDragging;
    private GdVector2 _offset;
    private GdVector2? _mouseDownPosition;

    private Node _originalParent = null!;
    private Node _dragAndDropLayer = null!;

    public bool IsInteractive { get; set; }

    public event Action SelectClicked = delegate { };
    public event Action DragStarted = delegate { };
    public event Action DragStopped = delegate { };

    public TextureRect TextureRect { get; private set; } = null!;
    public Control CountBackground { get; private set; } = null!;
    public Label CountLabel { get; private set; } = null!;

    public override void _Ready()
    {
        _originalParent = GetParent().Required();
        _dragAndDropLayer = GetNode("/root/GameplayMain/Ui/DragAndDropLayer").Required();
        TextureRect = GetNode<TextureRect>("TextureRect").Required();
        CountBackground = GetNode<Control>("TextureRect/CountBackground").Required();
        CountLabel = GetNode<Label>("TextureRect/CountBackground/CountLabel").Required();

        MouseEntered += () => _isMouseHovering = true;
        MouseExited += () => _isMouseHovering = false;
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
        _dragAndDropLayer.AddChild(this);
        GlobalPosition = position;
        DragStarted();
    }

    public void StopDrag()
    {
        _isDragging = false;
        _isMouseHovering = false;
        Position = GdVector2.Zero;
        GetParent().RemoveChild(this);
        _originalParent.AddChild(this);
        DragStopped();
    }
}
