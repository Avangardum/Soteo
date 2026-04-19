using System.Diagnostics.CodeAnalysis;

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
        
        public IReadOnlyKeyValuePair<TKey, TValue> AsReadOnly() =>
            new ReadOnlyKeyValuePairWrapper<TKey, TValue>(self);
    }
    
    extension<TKey, TValue> (IReadOnlyKeyValuePair<TKey, TValue> self)
    {
        public void Deconstruct(out TKey key, out TValue value)
        {
            key = self.Key;
            value = self.Value;
        }
    }
    
    extension<TKey, TValue> (IReadOnlyDictionary<TKey, TValue> self)
    {
        public TValue? GetOrDefault(TKey key, TValue? defaultValue = default) =>
            self.TryGetValue(key, out TValue? value) ? value : defaultValue;
        
        public ICovariantReadOnlyDictionary<TKey, TValue> AsCovariant() =>
            new CovariantReadOnlyDictionaryWrapper<TKey, TValue>(self);
    }
    
    extension<TKey, TValue> (ICovariantReadOnlyDictionary<TKey, TValue> self) where TValue : notnull
    {
        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
        {
            if (self.ContainsKey(key))
            {
                value = self[key];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
        
        public TValue? GetOrDefault(TKey key, TValue? defaultValue = default) =>
            self.TryGetValue(key, out TValue? value) ? value : defaultValue;
    }
}