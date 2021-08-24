using System;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Extensions
{
    public static class OptionalValueChangeExtensions
    {
        public static DateTime? MapDataTimeOptionalChangeWithFallback(this OptionalValueChange<Maybe<DateTime>> optionalChange, DateTime? fallback)
        {
            return optionalChange
                .Match(changeTo =>
                        changeTo.Match
                        (
                            newValue => newValue, //Client set new value
                            () => (DateTime?)null), //Changed to null by client
                    () => fallback // No change provided - use the fallback
                );
        }

        public static T MapOptionalChangeWithFallback<T>(this OptionalValueChange<T> optionalChange, T fallback)
        {
            return optionalChange
                .Match(changeTo =>
                        changeTo, //Changed to null by client
                    () => fallback // No change provided - use the fallback
                );
        }

        public static Result<TTarget, OperationError> WithOptionalUpdate<TTarget, TValue>(
            this TTarget target,
            OptionalValueChange<TValue> optionalUpdate,
            Func<TTarget, TValue, Result<TTarget, OperationError>> updateCommand)
        {
            return optionalUpdate.Match(changedValue => updateCommand(target, changedValue), () => target);
        }

        public static Result<TTarget, OperationError> WithOptionalUpdate<TTarget, TValue>(
            this TTarget target,
            Maybe<TValue> optionalUpdate,
            Func<TTarget, TValue, Result<TTarget, OperationError>> updateCommand)
        {
            return optionalUpdate
                .Select(changedValue => updateCommand(target, changedValue))
                .Match(updateResult => updateResult, () => target);
        }

        public static Result<TTarget, OperationError> WithOptionalUpdate<TTarget, TValue>(
            this TTarget target,
            OptionalValueChange<TValue> optionalUpdate,
            Func<TTarget, TValue, Maybe<OperationError>> updateCommand)
        {
            return optionalUpdate
                .Match
                (
                    fromChange: changedValue => updateCommand(target, changedValue).Match<Result<TTarget, OperationError>>(error => error, () => target),
                    fromNone: () => target
                );
        }

        public static Result<TTarget, OperationError> WithOptionalUpdate<TTarget, TValue>(
            this TTarget target,
            OptionalValueChange<TValue> optionalUpdate,
            Action<TTarget, TValue> updateCommand)
        {
            if (optionalUpdate.HasChange)
                updateCommand(target, optionalUpdate.NewValue);

            return target;
        }
    }
}
