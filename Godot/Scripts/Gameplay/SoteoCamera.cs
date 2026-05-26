using Soteo.Core.Gameplay;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Util;

namespace Soteo.Gameplay;

public sealed class SoteoCamera : Camera2D, ICamera
{
    [Export] private int _minZoomLog2;
    [Export] private int _maxZoomLog2;
    [Export] private int _defaultZoomLog2;
    [Export] private double _zoomLog2Step;
    [Export] private bool _roundZoomToInt;
    [Export] private double _scrollSpeed;
    [Export] private double _scrollZoneThickness;
    [Export] private double _limit;
    
    private GdVector2 _prevGlobalMousePos;
    private bool _wasDraggingInPrevFrame;
    
    public event Action ZoomChanged = delegate {};

    public new Vector2 Position
    {
        get => base.Position.ToSys();
        set => base.Position = value.ToGd();
    }

    /// <inheritdoc/>
    public double TrueZoom
    {
        get => 1 / Zoom.x;
        private set
        {
            if (value == TrueZoom) return;
            GdVector2 mousePosBefore = GetGlobalMousePosition();
            Zoom = GdVector2.One / value;
            base.Position += mousePosBefore - GetGlobalMousePosition();
            ZoomChanged();
        }
    }

    private double TargetZoomLog2
    {
        get;
        set
        {
            // When rounding zoom to int, dead zones appear near min and max values, in which resulting zoom is
            // the same. In this case, we cut these dead zones off, so that a single scroll tick from an edge
            // value changes the zoom.
            double min = _roundZoomToInt ? Math.Log(Math.Pow(2, _minZoomLog2) + 0.499f, 2) : _minZoomLog2;
            double max = _roundZoomToInt ? Math.Log(Math.Pow(2, _maxZoomLog2) - 0.499f, 2) : _maxZoomLog2;
            
            field = Maths.Clamp(value, min, max);
            double targetZoom = Math.Pow(2, field);
            TrueZoom = _roundZoomToInt ? Math.Round(targetZoom) : targetZoom;
        }
    }

    public override void _Ready()
    {
        TargetZoomLog2 = _defaultZoomLog2;
    }

    public override void _Process(float delta)
    {
        GdVector2 globalMousePos = GetGlobalMousePosition();
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
        RoundPositionToPixelPerfect();
        
        _prevGlobalMousePos = globalMousePos;
        _wasDraggingInPrevFrame = isDragging;
    }
    
    private void Drag(ref GdVector2 globalMousePos)
    {
        // Moving camera doesn't update GetGlobalMousePosition() immediately, so we track it manually
        
        GdVector2 deltaPos = _prevGlobalMousePos - globalMousePos;
        base.Position += deltaPos;
        globalMousePos += deltaPos;
    }
    
    private void Scroll(double delta)
    {
        if (OS.GetCmdlineArgs().Contains("--no-scroll")) return;
        GdVector2 viewportMousePos = GetViewport().GetMousePosition();
        GdVector2 viewportSize = GetViewport().GetVisibleRect().Size;
        int xDirection = viewportMousePos.x < _scrollZoneThickness ? -1 :
            viewportMousePos.x > viewportSize.x - _scrollZoneThickness ? 1 : 0;
        int yDirection = viewportMousePos.y < _scrollZoneThickness ? -1 :
            viewportMousePos.y > viewportSize.y - _scrollZoneThickness ? 1 : 0;
        base.Position += new GdVector2(xDirection, yDirection) * delta * _scrollSpeed / TrueZoom;
    }
    
    private void EnforceLimit()
    {
        // By default, limits work only visually, camera position can go over them, but visually stays in the bounds,
        // which breaks stuff dependent on camera position, so they are not used and a custom alternative is
        // implemented instead.
        
        GdVector2 viewportHalfSizeInWorldSpace = GetViewport().GetVisibleRect().Size / TrueZoom / 2;
        GdVector2 position = base.Position;
        
        double minX = -_limit + viewportHalfSizeInWorldSpace.x;
        double maxX = _limit - viewportHalfSizeInWorldSpace.x;
        if (minX > maxX) minX = maxX = 0;
        position.x = (float)Maths.Clamp(position.x, minX, maxX);
        
        double minY = -_limit + viewportHalfSizeInWorldSpace.y;
        double maxY = _limit - viewportHalfSizeInWorldSpace.y;
        if (minY > maxY) minY = maxY = 0;
        position.y = (float)Maths.Clamp(position.y, minY, maxY);
        
        base.Position = position;
    }
    
    private void RoundPositionToPixelPerfect()
    {
        GdVector2 viewportSize = GetViewport().GetVisibleRect().Size;
        bool halfPixelXOffset = viewportSize.x % 2 == 1;
        bool halfPixelYOffset = viewportSize.y % 2 == 1;
        Position = NodeHelper.RoundPositionToPixelPerfect(
            Position, TrueZoom, isCamera: true, halfPixelXOffset, halfPixelYOffset);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("zoom_in")) TargetZoomLog2 += _zoomLog2Step;
        else if (e.IsActionPressed("zoom_out")) TargetZoomLog2 -= _zoomLog2Step;
    }
}