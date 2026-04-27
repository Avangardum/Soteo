using Soteo.Gameplay.Interfaces;
using Soteo.Shared;

namespace Soteo.Gameplay;

public sealed class SoteoCamera : Camera2D, ICamera
{
    [Export] private int _minZoomLog2;
    [Export] private int _maxZoomLog2;
    [Export] private int _defaultZoomLog2;
    [Export] private float _zoomLog2Step;
    [Export] private bool _roundZoomToInt;
    [Export] private float _scrollSpeed;
    [Export] private float _scrollZoneThickness;
    [Export] private float _limit;
    
    private Vector2 _prevGlobalMousePos;
    private bool _wasDraggingInPrevFrame;
    
    public event Action ZoomChanged = delegate {};

    /// <inheritdoc/>
    public float TrueZoom
    {
        get => 1 / Zoom.x;
        private set
        {
            if (value == TrueZoom) return;
            Vector2 mousePosBefore = GetGlobalMousePosition();
            Zoom = Vector2.One / value;
            Position += mousePosBefore - GetGlobalMousePosition();
            ZoomChanged();
        }
    }

    private float TargetZoomLog2
    {
        get;
        set
        {
            // When rounding zoom to int, dead zones appear near min and max values, in which resulting zoom is
            // the same. In this case, we cut these dead zones off, so that a single scroll tick from an edge
            // value changes the zoom.
            float min = _roundZoomToInt ? SoteoMath.Log(Mathf.Pow(2, _minZoomLog2) + 0.499f, 2) : _minZoomLog2;
            float max = _roundZoomToInt ? SoteoMath.Log(Mathf.Pow(2, _maxZoomLog2) - 0.499f, 2) : _maxZoomLog2;
            
            field = Mathf.Clamp(value, min, max);
            float targetZoom = Mathf.Pow(2, field);
            TrueZoom = _roundZoomToInt ? Mathf.Round(targetZoom) : targetZoom;
        }
    }

    public override void _Ready()
    {
        TargetZoomLog2 = _defaultZoomLog2;
    }

    public override void _Process(float delta)
    {
        Vector2 globalMousePos = GetGlobalMousePosition();
        bool isDragging = Input.IsActionPressed("drag_camera");
        
        if (isDragging)
        {
            if (_wasDraggingInPrevFrame)
            {
                Drag(ref globalMousePos);
            }
        }
        else
        {
            Scroll(delta);
        }
        
        EnforceLimit();
        
        _prevGlobalMousePos = globalMousePos;
        _wasDraggingInPrevFrame = isDragging;
    }
    
    private void Drag(ref Vector2 globalMousePos)
    {
        // Moving camera doesn't update GetGlobalMousePosition() immediately, so we track it manually
        
        Vector2 deltaPos = _prevGlobalMousePos - globalMousePos;
        Position += deltaPos;
        globalMousePos += deltaPos;
    }
    
    private void Scroll(float delta)
    {
        if (OS.GetCmdlineArgs().Contains("--no-scroll")) return;
        Vector2 viewportMousePos = GetViewport().GetMousePosition();
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        int xDirection = viewportMousePos.x < _scrollZoneThickness ? -1 :
            viewportMousePos.x > viewportSize.x - _scrollZoneThickness ? 1 : 0;
        int yDirection = viewportMousePos.y < _scrollZoneThickness ? -1 :
            viewportMousePos.y > viewportSize.y - _scrollZoneThickness ? 1 : 0;
        Position += new Vector2(xDirection, yDirection) * delta * _scrollSpeed / TrueZoom;
    }
    
    private void EnforceLimit()
    {
        // By default, limits work only visually, camera position can go over them, but visually stays in the bounds,
        // which breaks stuff dependent on camera position, so they are not used and a custom alternative is
        // implemented instead.
        
        Vector2 viewportHalfSizeInWorldSpace = GetViewport().GetVisibleRect().Size / TrueZoom / 2;
        Vector2 position = Position;
        
        float minX = -_limit + viewportHalfSizeInWorldSpace.x;
        float maxX = _limit - viewportHalfSizeInWorldSpace.x;
        if (minX > maxX) minX = maxX = 0;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        
        float minY = -_limit + viewportHalfSizeInWorldSpace.y;
        float maxY = _limit - viewportHalfSizeInWorldSpace.y;
        if (minY > maxY) minY = maxY = 0;
        position.y = Mathf.Clamp(position.y, minY, maxY);
        
        Position = position;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("zoom_in")) TargetZoomLog2 += _zoomLog2Step;
        else if (e.IsActionPressed("zoom_out")) TargetZoomLog2 -= _zoomLog2Step;
    }
}