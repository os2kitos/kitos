using System;
using Infrastructure.Services.Types;

namespace Core.DomainModel.Result
{
    public class Result<TSuccess, TFailure>
    {
        private readonly Maybe<TFailure> _failure;
        private readonly Maybe<TSuccess> _value;

        public static implicit operator Result<TSuccess, TFailure>(TFailure failure)
        {
            return Failure(failure);
        }

        public static implicit operator Result<TSuccess, TFailure>(TSuccess successValue)
        {
            return Success(successValue);
        }

        protected Result(Maybe<TSuccess> successResult, Maybe<TFailure> failureResult)
        {
            if (successResult.HasValue == failureResult.HasValue)
            {
                throw new ArgumentException($"{nameof(successResult)} cannot return the same value of {nameof(successResult.HasValue)} as {nameof(failureResult)}");
            }
            Ok = successResult.HasValue;
            _value = successResult;
            _failure = failureResult;
        }

        public bool Ok { get; }

        public bool Failed => !Ok;

        public TSuccess Value => _value.Value;

        public TFailure Error => _failure.Value;

        /// <summary>
        /// Consider using implicit ctor in stead
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Result<TSuccess, TFailure> Success(TSuccess value)
        {
            return new Result<TSuccess, TFailure>(Maybe<TSuccess>.Some(value), Maybe<TFailure>.None);
        }

        /// <summary>
        /// Consider using the implicit ctor in stead
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Result<TSuccess, TFailure> Failure(TFailure value)
        {
            return new Result<TSuccess, TFailure>(Maybe<TSuccess>.None, Maybe<TFailure>.Some(value));
        }

        /// <summary>
        /// Applies the provided <paramref name="transform"/> to the successful value if present
        /// If the current state of the result is "error", the error is returned.
        /// Applies state-based decision logic on how to build the next result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Result<T, TFailure> Select<T>(Func<TSuccess, Result<T, TFailure>> transform)
        {
            return Match(transform, failure => failure);
        }

        public T Match<T>(Func<TSuccess, T> onSuccess, Func<TFailure, T> onFailure)
        {
            return Ok ? onSuccess(Value) : onFailure(Error);
        }

        public override string ToString()
        {
            return Ok ? "RESULT: SUCCESS" : $"RESULT: ERROR '{Error}'";
        }
    }
}
