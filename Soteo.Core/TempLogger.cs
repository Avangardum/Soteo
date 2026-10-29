namespace Soteo.Core;

public static class TempLogger // todo remove
{
    public static Action<string> Log { get; set; } = null!;
}
