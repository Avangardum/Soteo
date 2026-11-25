namespace Soteo.Main.Gameplay.Ui;

public sealed partial class Graph : Control
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
            new GdVector2(_dataRect.Position.X, Size.Y),
            new GdVector2(_dataRect.Position.X, 0),
            new GdVector2(_dataRect.Position.X + _dataRect.Size.X, 0),
            new GdVector2(_dataRect.Position.X + _dataRect.Size.X, Size.Y),
            new GdVector2(_dataRect.Position.X, Size.Y),
            new GdVector2(_dataRect.Position.X, _dataRect.Position.Y + _dataRect.Size.Y),
            new GdVector2(_dataRect.Position.X - _dataRect.Position.Y,
                _dataRect.Position.Y + _dataRect.Size.Y),
            new GdVector2(_dataRect.Position.X, _dataRect.Position.Y + _dataRect.Size.Y),
            new GdVector2(_dataRect.Position.X, _dataRect.Position.Y),
            new GdVector2(_dataRect.Position.X - _dataRect.Position.Y, _dataRect.Position.Y),
            new GdVector2(_dataRect.Position.X, _dataRect.Position.Y)
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
            GdVector2 point = GdVector2.New(x, y) * _dataRect.Size + _dataRect.Position;
            point.Y = Mathf.Clamp(point.Y, 0, Size.Y);
            dataPoints[i] = point;
        }

        _line.Points = [.._frame, ..dataPoints, dataPoints[^1] + new GdVector2(_dataRect.Position.Y, 0)];
        _lastLabel.Text = data[^1].ToString(format);
        _lastLabel.Position = _lastLabel.Position with { Y = dataPoints[^1].Y - _lastLabel.Size.Y / 2 };
    }
}
