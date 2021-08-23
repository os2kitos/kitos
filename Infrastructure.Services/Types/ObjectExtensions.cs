using System;
using System.Collections.Generic;

namespace Infrastructure.Services.Types
{
    public static class ObjectExtensions
    {
        public static Maybe<T> FromNullable<T>(this T src)
        {
            return src == null ? Maybe<T>.None : Maybe<T>.Some(src);
        }

        public static Maybe<string> FromString(this string src)
        {
            return string.IsNullOrEmpty(src) ? Maybe<string>.None : src;
        }
        public static string GetValueOrEmptyString(this Maybe<string> src)
        {
            return src.Match(val => val, () => string.Empty);
        }

        public static TOut Transform<TIn, TOut>(this TIn input, Func<TIn, TOut> transform)
        {
            return transform(input);
        }

        public static IEnumerable<T> WrapAsEnumerable<T>(this T source)
        {
            yield return source;
        }
    }
}
