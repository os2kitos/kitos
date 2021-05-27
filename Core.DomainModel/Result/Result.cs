using System;

namespace Core.DomainModel.Result
{
    public sealed class Result<TSuccess, TFailure>
    {
        private readonly IResultPath<TSuccess, TFailure> _state;

        private Result(IResultPath<TSuccess, TFailure> state)
        {
            _state = state;
        }

        public static implicit operator Result<TSuccess, TFailure>(TFailure failure) =>
            Failure(failure);

        public static implicit operator Result<TSuccess, TFailure>(TSuccess successValue) =>
            Success(successValue);

        public bool Ok => _state.Success;

        public bool Failed => !Ok;

        public TSuccess Value => _state.SuccessValue;

        public TFailure Error => _state.ErrorValue;

        public static Result<TSuccess, TFailure> Success(TSuccess value) =>
            new Result<TSuccess, TFailure>(new SuccessPath<TSuccess, TFailure>(value));


        public static Result<TSuccess, TFailure> Failure(TFailure value) =>
            new Result<TSuccess, TFailure>(new ErrorPath<TSuccess, TFailure>(value));

        /// <summary>
        /// Transform the success result to a new result where the <typeparam name="TSuccess" /> is transformed into <typeparam name="T" /> using <paramref name="onSuccess"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        public Result<T, TFailure> Select<T>(Func<TSuccess, T> onSuccess) =>
            Match(value => Result<T, TFailure>.Success(onSuccess(value)), failure => failure);

        /// <summary>
        /// Bind the result of the current Result with that of another. <typeparam name="TFailure" /> is shared between the two and <typeparam name="TSuccess" /> is input to the computation of the other result with <typeparam name="T" />
        /// If you wish to select on the current state, just use <see cref="Select{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        public Result<T, TFailure> Bind<T>(Func<TSuccess, Result<T, TFailure>> onSuccess) => Match(onSuccess, failure => failure);

        public T Match<T>(Func<TSuccess, T> onSuccess, Func<TFailure, T> onFailure) =>
            _state.Match(onSuccess, onFailure);

        public override string ToString() =>
            Match(_ => "SUCCESS", error => $"FAILED:'{Error}'");

        #region private state classes
        private interface IResultPath<out TSuccessPath, out TErrorPath>
        {
            bool Success { get; }
            TSuccessPath SuccessValue { get; }
            TErrorPath ErrorValue { get; }
            T Match<T>(Func<TSuccessPath, T> onSuccess, Func<TErrorPath, T> onFailure);
        }

        private sealed class ErrorPath<TSuccessPath, TErrorPath> : IResultPath<TSuccessPath, TErrorPath>
        {
            public ErrorPath(TErrorPath error)
            {
                ErrorValue = error;
            }

            public bool Success => false;

            public TSuccessPath SuccessValue =>
                throw new InvalidOperationException("Success value is invalid in this state");

            public TErrorPath ErrorValue { get; }

            public T Match<T>(Func<TSuccessPath, T> onSuccess, Func<TErrorPath, T> onFailure) =>
                onFailure(ErrorValue);
        }

        private sealed class SuccessPath<TSuccessPath, TErrorPath> : IResultPath<TSuccessPath, TErrorPath>
        {
            public SuccessPath(TSuccessPath result)
            {
                SuccessValue = result;
            }

            public bool Success => true;

            public TSuccessPath SuccessValue { get; }

            public TErrorPath ErrorValue => throw new InvalidOperationException("Error value is invalid in this state");

            public T Match<T>(Func<TSuccessPath, T> onSuccess, Func<TErrorPath, T> onFailure) =>
                onSuccess(SuccessValue);
        }
        #endregion
    }
}
