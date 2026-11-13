using Soteo.Core;
using Soteo.Core.Localization;

namespace Soteo.Main.Gameplay;

public sealed class Localizer : ILocalizer
{
    private readonly IReadOnlyDictionary<string, IPluralizer> _pluralizers;

    public Localizer(ITypeLocator typeLocator)
    {
        Dictionary<string, IPluralizer> pluralizers = [];
        foreach (IPluralizer pluralizer in typeLocator.InstanceSubclassesOf<IPluralizer>())
        {
            foreach (string languageCode in pluralizer.LanguageCodes)
            {
                pluralizers[languageCode] = pluralizer;
            }
        }
        _pluralizers = pluralizers.ToDictionary().AsReadOnly();
    }

    public string GetString(string key) => TranslationServer.Translate(key);

    public int GetPluralisationIndex(double? amount)
    {
        string languageCode = TranslationServer.GetLocale().Split("_")[0];
        return _pluralizers[languageCode].GetPluralisationIndex(amount);
    }
}
