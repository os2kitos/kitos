using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.ApplicationServices
{
    public static class LinqTreeExtension
    {
        public static IEnumerable<T> SelectNestedChildren<T>
            (this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            foreach (T item in source)
            {
                yield return item;
                foreach (T subItem in SelectNestedChildren(selector(item), selector))
                {
                    yield return subItem;
                }
            }
        }

        public static IEnumerable<T> SelectNestedParents<T>
            (this T source, Func<T, T> selector)
            where T : class
        {
            yield return source;
            var parent = selector(source);
            if (parent == null)
                yield break;
            yield return SelectNestedParents(parent, selector).FirstOrDefault();
        }
    }
}