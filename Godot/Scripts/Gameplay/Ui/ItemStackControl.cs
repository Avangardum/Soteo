using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed class ItemStackControl : Control
{
    private bool _isMouseHovering;
    private bool _isDragging;
    private GdVector2 _offset;
    private readonly LateInit<Node> _originalParent = new();
    private readonly LateInit<Node> _dragAndDropLayer = new();

    public event Action DragStarted = delegate { };
    public event Action DragStopped = delegate { };

    public override void _Ready()
    {
        _originalParent.Value = GetParent().Required;
        _dragAndDropLayer.Value = GetNode("/root/GameplayMain/Ui/DragAndDropLayer").Required;
        this.Connect("mouse_entered", () => _isMouseHovering = true);
        this.Connect("mouse_exited", () => _isMouseHovering = false);
    }

    public override void _Process(float delta)
    {
        if (_isMouseHovering && Input.IsActionJustPressed("select"))
            StartDrag();
        else if (Input.IsActionJustReleased("select"))
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

    private void StopDrag()
    {
        _isDragging = false;
        RectPosition = GdVector2.Zero;
        GetParent().RemoveChild(this);
        _originalParent.Value.AddChild(this);
        DragStopped();
    }
}
