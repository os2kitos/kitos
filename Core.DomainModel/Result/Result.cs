﻿using System;

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

        public override string ToString()
        {
            return Ok ? "RESULT: SUCCESS" : $"RESULT: ERROR '{Error}'";
        }
    }
}
