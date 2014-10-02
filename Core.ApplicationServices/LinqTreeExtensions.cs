using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Core.ApplicationServices
{
    public static class LinqTreeExtension
    {
       public static IEnumerable<T> SelectNestedChildren<T>
            (this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            foreach (var item in source)
            {
                yield return item;
                foreach (var subItem in SelectNestedChildren(selector(item), selector))
                {
                    yield return subItem;
                }
            }
        }

        public static IEnumerable<T> SelectNestedParents<T>
            (this T source, Func<T, T> selector)
            where T : class
        {
            var current = selector(source);
            while (current != null)
            {
                yield return current;
                current = selector(current);
            }
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

    public static class LinqToCSV
    {
        public static string ToCsv<T>(this IEnumerable<T> items)
            where T : class
        {
            var csvBuilder = new StringBuilder();
            var properties = typeof(T).GetProperties();
            foreach (T item in items)
            {
                string line;
                if (item is ExpandoObject)
                {
                    // typeof(T).GetProperties() doesn't work with ExpandoObject so instead we need to cast to IDictionary
                    var propertyValues = item as IDictionary<string, object>;
                    line = string.Join(";", propertyValues.Values.Select(p => p.ToCsvValue()).ToArray());
                }
                else
                {
                    line = string.Join(";", properties.Select(p => p.GetValue(item, null).ToCsvValue()).ToArray());
                }
                csvBuilder.AppendLine(line);
            }
            return csvBuilder.ToString();
        }

        private static string ToCsvValue<T>(this T item)
        {
            if (item == null) return "\"\"";

            if (item is string)
            {
                return string.Format("\"{0}\"", item.ToString().Replace("\"", "\\\""));
            }
            double dummy;
            if (double.TryParse(item.ToString(), out dummy))
            {
                return string.Format("{0}", item);
            }
            return string.Format("\"{0}\"", item);
        }
    }
}