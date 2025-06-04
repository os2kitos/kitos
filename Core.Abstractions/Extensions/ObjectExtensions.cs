using System;
using System.Collections.Generic;
using Core.Abstractions.Types;

namespace Core.Abstractions.Extensions
{
    public static class ObjectExtensions
    {
        public static Maybe<T> FromNullable<T>(this T src)
        {
            return src == null ? Maybe<T>.None : Maybe<T>.Some(src);
        }

        public static Maybe<T> FromNullableValueType<T>(this T? src) where T : struct
        {
            return src == null ? Maybe<T>.None : Maybe<T>.Some(src.Value);
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

        public static T? GetValueOrNull<T>(this Maybe<T> source) where T : struct
        {
            return source.HasValue ? source.Value : null;
        }
    }
}
