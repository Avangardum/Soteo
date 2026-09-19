namespace Soteo.Core.Interfaces;

public interface IPluralizer
{
    IReadOnlyList<string> LanguageCodes { get; }
    
    /// <summary>
    /// Get index used to select a pluralization variant from localization for a given amount.
    /// Null means a value which is not representable by a single number, such as a multi-value Scalable.
    /// </summary>
    int GetPluralisationIndex(double? amount);
}
