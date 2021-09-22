using Core.Abstractions.Types;

namespace Core.Abstractions.Extensions
{
    public static class ResultExtensions
    {
        public static Maybe<TFailure> MatchFailure<TSuccess, TFailure>(this Result<TSuccess, TFailure> src)
        {
            return src.Match(_ => Maybe<TFailure>.None, error => error);
        }
    }
}
