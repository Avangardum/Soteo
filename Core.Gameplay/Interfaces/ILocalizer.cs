namespace Soteo.Core.Gameplay.Interfaces;

public interface ILocalizer
{
    string GetString(string key);
    
    int GetPluralisationIndex(double? amount);
}