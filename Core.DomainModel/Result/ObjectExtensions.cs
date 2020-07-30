using System;

namespace Core.DomainModel.Result
{
    public static class ObjectExtensions
    {
        public static Maybe<T> FromNullable<T>(this T src)
        {
            return src == null ? Maybe<T>.None : Maybe<T>.Some(src);
        }

        public static Maybe<string> FromString(this string src)
        {
            return src
                .FromNullable()
                .Select(value => string.IsNullOrEmpty(value) ? default(string) : value);
        }

        public static TOut Transform<TIn, TOut>(this TIn input, Func<TIn, TOut> transform)
        {
            return transform(input);
        }
    }
}
