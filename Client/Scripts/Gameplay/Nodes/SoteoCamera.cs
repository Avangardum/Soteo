using Soteo.Shared;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes;

public sealed class SoteoCamera : Camera2D
{
    [Export] private float _minZoomLog2;
    [Export] private float _maxZoomLog2;
    [Export] private float _defaultZoomLog2;
    [Export] private float _zoomLog2Step;
    [Export] private float _scrollSpeed;
    [Export] private float _scrollZoneThickness;
    [Export] private float _limit;
    
    private Vector2 _prevGlobalMousePos;
    private bool _wasDraggingInPrevFrame;
    
    private float ZoomLog2
    {
        get => SoteoMath.Log(this.TrueZoom.x, 2);
        set
        {
            Vector2 mousePosBefore = GetGlobalMousePosition();
            this.TrueZoom = Vector2.One * Mathf.Pow(2, value);
            Position += mousePosBefore - GetGlobalMousePosition();
        }
    }

    public override void _Ready()
    {
        ZoomLog2 = _defaultZoomLog2;
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
        Vector2 viewportMousePos = GetViewport().GetMousePosition();
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        int xDirection = viewportMousePos.x < _scrollZoneThickness ? -1 :
            viewportMousePos.x > viewportSize.x - _scrollZoneThickness ? 1 : 0;
        int yDirection = viewportMousePos.y < _scrollZoneThickness ? -1 :
            viewportMousePos.y > viewportSize.y - _scrollZoneThickness ? 1 : 0;
        Position += new Vector2(xDirection, yDirection) * delta * _scrollSpeed / this.TrueZoom;
    }
    
    private void EnforceLimit()
    {
        // By default, limits work only visually, camera position can go over them, but visually stays in the bounds,
        // which breaks stuff dependent on camera position, so they are not used and a custom alternative is
        // implemented instead.
        
        Vector2 viewportHalfSizeInWorldSpace = GetViewport().GetVisibleRect().Size / this.TrueZoom / 2;
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
        if (e.IsActionPressed("zoom_in"))
            ZoomLog2 = Mathf.Clamp(ZoomLog2 + _zoomLog2Step, _minZoomLog2, _maxZoomLog2);
        else if (e.IsActionPressed("zoom_out"))
            ZoomLog2 = Mathf.Clamp(ZoomLog2 - _zoomLog2Step, _minZoomLog2, _maxZoomLog2);
    }
}