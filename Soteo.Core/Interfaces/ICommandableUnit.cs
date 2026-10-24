namespace Soteo.Core.Interfaces;

internal interface ICommandableUnit : IUnit
{
    void SetCommand(ICommand command);
}
