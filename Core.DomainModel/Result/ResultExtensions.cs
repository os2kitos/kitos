using System;

namespace Core.DomainModel.Result
{
    public static class ResultExtensions
    {
        public static T Match<T, TSuccess, TFailure>(
            this Result<TSuccess, TFailure> src, 
            Func<TSuccess, T> onSuccess,
            Func<TFailure, T> onFailure)
        {
            return src.Ok ? onSuccess(src.Value) : onFailure(src.Error);
        }
    }
}
