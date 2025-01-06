using System.Collections.Concurrent;

namespace Bliztard.Application.Extension;

public static class ConcurrentDictionaryExtensions
{
    public static ConcurrentDictionary<TKey, TValue> TryAddAndReturn<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        dictionary.TryAdd(key, value);
        
        return dictionary;
    }
}
