namespace Soteo.Core.Interfaces;

public interface IPluralizer
{
    IReadOnlyList<string> LanguageCodes { get; }
    int GetPluralisationIndex(double? amount);
}
