namespace Soteo.Core.Items;

public sealed class LogItem : Item
{
    public override string IconPath => "Placeholder"; // todo show error if path is wrong (item/ability/status)

    public override long MaxStackCount => 10;
}
