using Soteo.Core.Commands;

namespace Soteo.Core.Entities.Units;

public sealed record UnitAiContext
{
    private readonly Unit _unit;

    public UnitAiContext(Unit unit)
    {
        _unit = unit;
    }

    public void SetCommand(Command command) => _unit.SetCommand(command);
}
