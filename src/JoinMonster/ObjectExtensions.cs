using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JoinMonster
{
    internal static class ObjectExtensions
    {
        public static IDictionary<string, object> ToDictionary(this object input)
        {
            switch (input)
            {
                case null:
                    throw new ArgumentNullException(nameof(input));
                case IDictionary<string, object> dictionary:
                    return dictionary;
                case IEnumerable<KeyValuePair<string, object>> enumerable:
                    return enumerable.ToDictionary(x => x.Key, x => x.Value);
            }

            var dict = new Dictionary<string, object>();

            foreach (var propertyInfo in input.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = propertyInfo.GetValue(input);
                dict.Add(propertyInfo.Name, value);
            }

            return dict;
        }
    }
}
