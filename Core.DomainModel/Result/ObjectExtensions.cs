namespace Core.DomainModel.Result
{
    public static class ObjectExtensions
    {
        public static Maybe<T> FromNullable<T>(this T src)
        {
            return src == null ? Maybe<T>.None : Maybe<T>.Some(src);
        }
    }
}
