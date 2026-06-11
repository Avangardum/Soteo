using System.Diagnostics.CodeAnalysis;
using Soteo.Util.Interfaces;

namespace Soteo.Util.Extensions;

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
        public TValue GetOrDefault(TKey key, TValue defaultValue) =>
            self.TryGetValue(key, out TValue? value) ? value : defaultValue;
        
        public TValue? GetOrDefault(TKey key) =>
            self.TryGetValue(key, out TValue? value) ? value : default;
        
        /// <summary>
        /// Create a copy of this dictionary with the specified values added / updated
        /// </summary>
        public IReadOnlyDictionary<TKey, TValue> With(IReadOnlyDictionary<TKey, TValue> values)
        {
            Dictionary<TKey, TValue> dict = self.ToDictionary();
            foreach ((TKey key, TValue value) in values)
                dict[key] = value;
            return dict;
        }
        
        /// <summary>
        /// Create a copy of this dictionary with the specified keys removed
        /// </summary>
        public IReadOnlyDictionary<TKey, TValue> Without(params IEnumerable<TKey> keys)
        {
            Dictionary<TKey, TValue> dict = self.ToDictionary();
            foreach (TKey key in keys)
                dict.Remove(key);
            return dict;
        }
    }

    extension<TKey, TValue, TTarget>(IReadOnlyDictionary<TKey, TValue> self) where TValue : TTarget
    {
        public IReadOnlyDictionary<TKey, TTarget> CovariantCast() =>
            new CovariantDictionaryWrapper<TKey, TTarget, TValue>(self);
    }

    extension<TKey, TValue> (IReadOnlyDictionary<TKey, TValue> self) where TValue : struct
    {
        public TValue? GetOrNull(TKey key) => self.TryGetValue(key, out TValue value) ? value : null; 
    }
    
    extension<TKey, TValue> (IDictionary<TKey, TValue> self)
    {
        public TValue GetOrAdd(TKey key, Func<TValue> valueFactory)
        {
            if (self.TryGetValue(key, out TValue? value)) return value;
            value = valueFactory();
            self[key] = value;
            return value;
        }
    }
    
    extension<TKey, TValue> (IEnumerable<KeyValuePair<TKey, TValue>> self)
    {
        public Dictionary<TKey, TValue> ToDictionary() =>
            self.ToDictionary(it => it.Key, it => it.Value);
    }
}
