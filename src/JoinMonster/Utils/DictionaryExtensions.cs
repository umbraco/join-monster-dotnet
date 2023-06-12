using System;
using System.Collections.Generic;

namespace JoinMonster.Utils;

internal static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
    {
        if (dictionary.TryGetValue(key, out TValue existingValue))
        {
            return existingValue;
        }
        else
        {
            TValue newValue = valueFactory();
            dictionary.Add(key, newValue);
            return newValue;
        }
    }
}
