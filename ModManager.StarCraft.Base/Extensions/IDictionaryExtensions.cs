using System.Collections.Generic;

namespace ModManager.StarCraft.Base
{
    public static class IDictionaryExtensions
    {
        public static TValue TryGetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue
        )
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }
}
