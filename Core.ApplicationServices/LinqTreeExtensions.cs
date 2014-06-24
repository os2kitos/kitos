using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

    public static class QueryableExtension
    {
        public static IQueryable<T> OrderByField<T>(this IQueryable<T> q, string sortField, bool descending = false)
        {
            var memberTokens = sortField.Split('.');

            var param = Expression.Parameter(typeof(T), "p");
            var property = (Expression) param;
            foreach (var token in memberTokens)
            {
                property = Expression.Property(property, token);
            }

            var exp = Expression.Lambda(property, param);
            var method = descending ? "OrderByDescending" : "OrderBy";
            var types = new Type[] { q.ElementType, exp.Body.Type };
            var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
            return q.Provider.CreateQuery<T>(mce);
        }
    }
}