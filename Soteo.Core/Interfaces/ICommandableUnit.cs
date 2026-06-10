namespace Soteo.Core.Interfaces;

internal interface ICommandableUnit : IUnit
{
    public void SetCommand(ICommand command);
}
