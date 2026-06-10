namespace Soteo.Core.Gameplay.Interfaces;

internal interface ICommandableUnit : IUnit
{
    public void SetCommand(ICommand command);
}
