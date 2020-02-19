using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Enumerations
{
    public static class IDictionaryExtensions
    {
        public static T Field<T>(this IDictionary<string, object> dictionary, string index)
        {
            var type = typeof(T);
            var obj = dictionary[index];
            if (typeof(Enumeration).IsAssignableFrom(type))
                return (T) Enumeration.GuessFrom(obj, type);

            return (T) Convert.ChangeType(obj, type);
        }

        public static TValue GetKeyOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetKeyOrDefault(key, default);
        }

        public static TValue GetKeyOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetKeyOrDefault(key);
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TValue> expr)
        {
            if (dictionary is ConcurrentDictionary<TKey, TValue> cDict) return cDict.GetOrAdd(key, x => expr.Invoke());

            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = expr.Invoke();
                dictionary.Add(key, value);
            }

            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            return dictionary.GetOrAdd(key, () => value);
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TKey, TValue> expr)
        {
            var cDict = dictionary as ConcurrentDictionary<TKey, TValue>;
            if (cDict != null) return cDict.GetOrAdd(key, expr.Invoke);

            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = expr.Invoke(key);
                dictionary.Add(key, value);
            }

            return value;
        }
    }
}