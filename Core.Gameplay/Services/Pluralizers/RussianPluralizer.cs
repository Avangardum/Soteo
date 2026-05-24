using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Services.Pluralizers;

public sealed class RussianPluralizer : IPluralizer
{
    private const int One = 0; // 1 секунда
    private const int Few = 1; // 2 секунды
    private const int Many = 2; // 5 секунд
    
    public IReadOnlyList<string> LanguageCodes => ["ru"];

    public int GetPluralisationIndex(double? amount)
    {
        if (amount == null || amount.Value % 1 != 0) return Many;
        double penultimateDigit = amount.Value / 10 % 10;
        if (penultimateDigit == 1) return Many;
        double lastDigit = amount.Value % 10;
        if (lastDigit == 1) return One;
        if (lastDigit is 2 or 3 or 4) return Few;
        return Many;
    }
}