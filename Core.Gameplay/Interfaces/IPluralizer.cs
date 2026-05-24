namespace Soteo.Core.Gameplay.Interfaces;

public interface IPluralizer
{
    IReadOnlyList<string> LanguageCodes { get; }
    
    int GetPluralisationIndex(double? amount);
}