using Soteo.Main.Shared.Extensions;
using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed partial class Graph : Control
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
            new GdVector2(DataRect.Position.X, Size.Y),
            new GdVector2(DataRect.Position.X, 0),
            new GdVector2(DataRect.Position.X + DataRect.Size.X, 0),
            new GdVector2(DataRect.Position.X + DataRect.Size.X, Size.Y),
            new GdVector2(DataRect.Position.X, Size.Y),
            new GdVector2(DataRect.Position.X, DataRect.Position.Y + DataRect.Size.Y),
            new GdVector2(DataRect.Position.X - DataRect.Position.Y,
                DataRect.Position.Y + DataRect.Size.Y),
            new GdVector2(DataRect.Position.X, DataRect.Position.Y + DataRect.Size.Y),
            new GdVector2(DataRect.Position.X, DataRect.Position.Y),
            new GdVector2(DataRect.Position.X - DataRect.Position.Y, DataRect.Position.Y),
            new GdVector2(DataRect.Position.X, DataRect.Position.Y)
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
            GdVector2 point = GdVector2.New(x, y) * DataRect.Size + DataRect.Position;
            point.Y = Mathf.Clamp(point.Y, 0, Size.Y);
            dataPoints[i] = point;
        }

        Line.Points = [..Frame, ..dataPoints, dataPoints[^1] + new GdVector2(DataRect.Position.Y, 0)];
        LastLabel.Text = data[^1].ToString(format);
        LastLabel.Position = LastLabel.Position with { Y = dataPoints[^1].Y - LastLabel.Size.Y / 2 };
    }
}
