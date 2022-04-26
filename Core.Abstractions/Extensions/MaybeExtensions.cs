using Core.Abstractions.Exceptions;
using Core.Abstractions.Types;

namespace Core.Abstractions.Extensions
{
    public static class MaybeExtensions
    {
        /// <summary>
        /// Throws an <see cref="OperationErrorException{TValue}"/> containing the value of the maybe
        /// If there is no value, no exception will be thrown
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="src"></param>
        public static void ThrowOnValue<TValue>(this Maybe<TValue> src)
        {
            src
                .Match
                (
                    onValue: value => throw new OperationErrorException<TValue>(value),
                    onNone: () => "Success"
                );
        }
    }
}
