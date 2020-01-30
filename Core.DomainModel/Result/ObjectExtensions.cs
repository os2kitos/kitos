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
    }
}
