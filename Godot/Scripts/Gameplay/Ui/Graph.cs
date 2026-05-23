using Soteo.Shared;

namespace Soteo.Gameplay.Ui;

public sealed class Graph : Control
{
    private Control _dataRect = null!;
    private Line2D _line = null!;
    private Label _minLabel = null!;
    private Label _maxLabel = null!;
    private Label _lastLabel = null!;
    
    private GdVector2[] _frame = null!;

    public override void _Ready()
    {
        _dataRect = GetNode<Control>("DataRect");
        _line = GetNode<Line2D>("Line2D");
        _minLabel = GetNode<Label>("Min");
        _maxLabel = GetNode<Label>("Max");
        _lastLabel = GetNode<Label>("Last");
        
        _frame = 
        [
            new GdVector2(_dataRect.RectPosition.x, RectSize.y),
            new GdVector2(_dataRect.RectPosition.x, 0),
            new GdVector2(_dataRect.RectPosition.x + _dataRect.RectSize.x, 0),
            new GdVector2(_dataRect.RectPosition.x + _dataRect.RectSize.x, RectSize.y),
            new GdVector2(_dataRect.RectPosition.x, RectSize.y),
            new GdVector2(_dataRect.RectPosition.x, _dataRect.RectPosition.y + _dataRect.RectSize.y),
            new GdVector2(_dataRect.RectPosition.x - _dataRect.RectPosition.y,
                _dataRect.RectPosition.y + _dataRect.RectSize.y),
            new GdVector2(_dataRect.RectPosition.x, _dataRect.RectPosition.y + _dataRect.RectSize.y),
            new GdVector2(_dataRect.RectPosition.x, _dataRect.RectPosition.y),
            new GdVector2(_dataRect.RectPosition.x - _dataRect.RectPosition.y, _dataRect.RectPosition.y),
            new GdVector2(_dataRect.RectPosition.x, _dataRect.RectPosition.y),
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
        _minLabel.Text = min.Value.ToString(format);
        _maxLabel.Text = max.Value.ToString(format);
        for (int i = 0; i < data.Count; i++)
        {
            double x = i / (data.Count - 1.0);
            double y = Maths.InverseLerp(max.Value, min.Value, data[i]);
            GdVector2 point = GdVector2.New(x, y) * _dataRect.RectSize + _dataRect.RectPosition;
            point.y = Mathf.Clamp(point.y, 0, RectSize.y);
            dataPoints[i] = point;
        }
        
        _line.Points = [.._frame, ..dataPoints, dataPoints[^1] + new GdVector2(_dataRect.RectPosition.y, 0)];
        _lastLabel.Text = data[^1].ToString(format);
        _lastLabel.RectPosition = _lastLabel.RectPosition with { y = dataPoints[^1].y - _lastLabel.RectSize.y / 2 };
    }
}