using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Services.Pluralizers;

public sealed class EnglishPluralizer : IPluralizer
{
    private const int One = 0; // 1 second
    private const int Many = 1; // 2 seconds
    
    public IReadOnlyList<string> LanguageCodes => ["en"];

    public int GetPluralisationIndex(double? amount) => amount == 1 ? One : Many;
}