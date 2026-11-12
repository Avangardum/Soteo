namespace Soteo.Core.Localization;

public interface ILocalizer
{
    string GetString(string key);
    int GetPluralisationIndex(double? amount);
}
