namespace Soteo.Core.Interfaces;

public interface ILocalizer
{
    string GetString(string key);
    int GetPluralisationIndex(double? amount);
}
