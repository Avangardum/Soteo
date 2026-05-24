using System.Collections.Immutable;
using Soteo.CampaignServer;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Services;

public sealed class Localizer : ILocalizer
{
    private readonly ImmutableDictionary<string, IPluralizer> _pluralizers;

    public Localizer()
    {
        Dictionary<string, IPluralizer> pluralizers = [];
        foreach (IPluralizer pluralizer in TypeLocator.InstanceAllSubclasses<IPluralizer>())
        {
            foreach (string languageCode in pluralizer.LanguageCodes)
            {
                pluralizers[languageCode] = pluralizer;
            }
        }
        _pluralizers = pluralizers.ToImmutableDictionary();
    }
    
    public string GetString(string key) => TranslationServer.Translate(key);

    public int GetPluralisationIndex(double? amount)
    {
        string languageCode = TranslationServer.GetLocale().Split("_")[0];
        return _pluralizers[languageCode].GetPluralisationIndex(amount);
    }
}