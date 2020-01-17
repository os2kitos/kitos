using Core.DomainServices.Model.Result;

namespace Core.DomainServices.Extensions
{
    public static class ObjectExtensions
    {
        public static Maybe<T> FromNullable<T>(this T src)
        {
            return src == null ? Maybe<T>.None : Maybe<T>.Some(src);
        }
    }
}
