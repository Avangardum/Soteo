using Soteo.Main.Shared.Extensions;
using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed class Graph : Control
{
    private readonly LateInit<Control> _dataRect = new();
    private readonly LateInit<Line2D> _line = new();
    private readonly LateInit<Label> _minLabel = new();
    private readonly LateInit<Label> _maxLabel = new();
    private readonly LateInit<Label> _lastLabel = new();
    private readonly LateInit<GdVector2[]> _frame = new();
    
    private Control DataRect => _dataRect;
    private Line2D Line => _line;
    private Label MinLabel => _minLabel;
    private Label MaxLabel => _maxLabel;
    private Label LastLabel => _lastLabel;
    private GdVector2[] Frame => _frame;
    
    public override void _Ready()
    {
        _dataRect.Value = GetNode<Control>("DataRect");
        _line.Value = GetNode<Line2D>("Line2D");
        _minLabel.Value = GetNode<Label>("Min");
        _maxLabel.Value = GetNode<Label>("Max");
        _lastLabel.Value = GetNode<Label>("Last");
        
        _frame.Value = 
        [
            new GdVector2(DataRect.RectPosition.x, RectSize.y),
            new GdVector2(DataRect.RectPosition.x, 0),
            new GdVector2(DataRect.RectPosition.x + DataRect.RectSize.x, 0),
            new GdVector2(DataRect.RectPosition.x + DataRect.RectSize.x, RectSize.y),
            new GdVector2(DataRect.RectPosition.x, RectSize.y),
            new GdVector2(DataRect.RectPosition.x, DataRect.RectPosition.y + DataRect.RectSize.y),
            new GdVector2(DataRect.RectPosition.x - DataRect.RectPosition.y,
                DataRect.RectPosition.y + DataRect.RectSize.y),
            new GdVector2(DataRect.RectPosition.x, DataRect.RectPosition.y + DataRect.RectSize.y),
            new GdVector2(DataRect.RectPosition.x, DataRect.RectPosition.y),
            new GdVector2(DataRect.RectPosition.x - DataRect.RectPosition.y, DataRect.RectPosition.y),
            new GdVector2(DataRect.RectPosition.x, DataRect.RectPosition.y),
        ];
        
        SetData([0, 0], "N0", 0, 1);
    }
    
    public void SetData(IReadOnlyList<double> data, string format, double? min = null, double? max = null)
    {
        if (data.Count < 2)
            data = [data.FirstOrDefault(), data.FirstOrDefault()];
        
        var dataPoints = new GdVector2[data.Count];
        min ??= data.Min();
        max ??= data.Max();
        MinLabel.Text = min.Value.ToString(format);
        MaxLabel.Text = max.Value.ToString(format);
        for (int i = 0; i < data.Count; i++)
        {
            double x = i / (data.Count - 1.0);
            double y = Maths.InverseLerp(max.Value, min.Value, data[i]);
            GdVector2 point = GdVector2.New(x, y) * DataRect.RectSize + DataRect.RectPosition;
            point.y = Mathf.Clamp(point.y, 0, RectSize.y);
            dataPoints[i] = point;
        }
        
        Line.Points = [..Frame, ..dataPoints, dataPoints[^1] + new GdVector2(DataRect.RectPosition.y, 0)];
        LastLabel.Text = data[^1].ToString(format);
        LastLabel.RectPosition = LastLabel.RectPosition with { y = dataPoints[^1].y - LastLabel.RectSize.y / 2 };
    }
}
