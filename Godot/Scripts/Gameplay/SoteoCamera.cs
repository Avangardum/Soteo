using Soteo.Core;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;
using Soteo.Util;

namespace Soteo.Main.Gameplay;

public sealed class SoteoCamera : Camera2D, ICamera
{
    private const int MinZoomLog2 = 0;
    private const int MaxZoomLog2 = 5;
    private const int DefaultZoomLog2 = 2;
    private const double ZoomLog2Step = 0.125;
    private const bool RoundZoomToInt = true;
    private const double ScrollSpeed = 720;
    private const double ScrollZoneThickness = -1;
    private const double Limit = 1000;
    
    private GdVector2 _prevGlobalMousePos;
    private bool _wasDraggingInPrevFrame;

    public event Action ZoomChanged = delegate { };
    
    public new Vector2 Position
    {
        get => base.Position.ToSys();
        set => base.Position = value.ToGd();
    }
    
    /// <inheritdoc/>
    public new double Zoom
    {
        get => 1 / base.Zoom.x;
        private set
        {
            if (value == Zoom) return;
            GdVector2 mousePosBefore = GetGlobalMousePosition();
            base.Zoom = GdVector2.One / value;
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
            // the same. In this case, these dead zones are cut off, so that a single scroll tick from an edge
            // value changes the zoom.
            double min = RoundZoomToInt ? Maths.Log(Maths.Pow(2, MinZoomLog2) + 0.499, 2) : MinZoomLog2;
            double max = RoundZoomToInt ? Maths.Log(Maths.Pow(2, MaxZoomLog2) - 0.499, 2) : MaxZoomLog2;
            
            field = Maths.Clamp(value, min, max);
            double targetZoom = Math.Pow(2, field);
            Zoom = RoundZoomToInt ? Math.Round(targetZoom) : targetZoom;
        }
    }

    public override void _Ready()
    {
        PauseMode = PauseModeEnum.Process;
        Current = true;
        TargetZoomLog2 = DefaultZoomLog2;
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
        if (ClientCmdLineArgs.NoScroll) return;
        GdVector2 viewportMousePos = GetViewport().GetMousePosition();
        GdVector2 viewportSize = GetViewport().GetVisibleRect().Size;
        int xDirection = viewportMousePos.x < ScrollZoneThickness ? -1 :
            viewportMousePos.x > viewportSize.x - ScrollZoneThickness ? 1 : 0;
        int yDirection = viewportMousePos.y < ScrollZoneThickness ? -1 :
            viewportMousePos.y > viewportSize.y - ScrollZoneThickness ? 1 : 0;
        base.Position += new GdVector2(xDirection, yDirection) * delta * ScrollSpeed / Zoom;
    }
    
    private void EnforceLimit()
    {
        // By default, limits work only visually, camera position can go over them, but visually stays in the bounds,
        // which breaks stuff dependent on camera position, so they are not used and a custom alternative is
        // implemented instead.
        
        GdVector2 viewportHalfSizeInWorldSpace = GetViewport().GetVisibleRect().Size / Zoom / 2;
        GdVector2 position = base.Position;
        
        double minX = -Limit + viewportHalfSizeInWorldSpace.x;
        double maxX = Limit - viewportHalfSizeInWorldSpace.x;
        if (minX > maxX) minX = maxX = 0;
        position.x = (float)Maths.Clamp(position.x, minX, maxX);
        
        double minY = -Limit + viewportHalfSizeInWorldSpace.y;
        double maxY = Limit - viewportHalfSizeInWorldSpace.y;
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
            Position, Zoom, isCamera: true, halfPixelXOffset, halfPixelYOffset);
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("zoom_in"))
            TargetZoomLog2 += ZoomLog2Step;
        else if (e.IsActionPressed("zoom_out"))
            TargetZoomLog2 -= ZoomLog2Step;
    }
}
