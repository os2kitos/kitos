using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core.DomainModel.Result
{
    public static class DictionaryExtensions
    {
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> input)
        {
            return
                input
                    .FromNullable()
                    .Select(dic => new ReadOnlyDictionary<TKey, TValue>(dic))
                    .GetValueOrDefault();
        }
    }
}
