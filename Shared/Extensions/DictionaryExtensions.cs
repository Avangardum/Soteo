namespace Soteo.Shared.Extensions;

public static class DictionaryExtensions
{
    extension<TKey, TValue> (KeyValuePair<TKey, TValue> self)
    {
        public void Deconstruct(out TKey key, out TValue value)
        {
            key = self.Key;
            value = self.Value;
        }
    }
    
    extension<TKey, TValue> (IDictionary<TKey, TValue> self)
    {
        public TValue? GetOrDefault(TKey key, TValue? defaultValue = default) =>
            self.TryGetValue(key, out TValue? value) ? value : defaultValue;
    }
}