using Core.Abstractions.Types;
using Core.Abstractions.Exceptions;

namespace Core.Abstractions.Extensions
{
    public static class ResultExtensions
    {
        /// <summary>
        /// Returns a <see cref="Maybe{TFailure}"/> which has a value if the <paramref name="src"/> contains a failed result.
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Maybe<TFailure> MatchFailure<TSuccess, TFailure>(this Result<TSuccess, TFailure> src)
        {
            return src.Match(_ => Maybe<TFailure>.None, error => error);
        }

        /// <summary>
        /// Throws an <see cref="OperationErrorException{TFailure}"/> if the <paramref name="src"/> contains a failed result.
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TFailure"></typeparam>
        /// <param name="src"></param>
        public static void ThrowOnFailure<TSuccess, TFailure>(this Result<TSuccess, TFailure> src)
        {
            src.MatchFailure().ThrowOnValue();
        }
    }
}
