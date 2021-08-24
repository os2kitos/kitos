using System;
using Core.ApplicationServices.Extensions;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.Shared
{
    //TODO: Use me
    /// <summary>
    /// Declares an optional change to a value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OptionalValueChange<T>
    {
        public static readonly OptionalValueChange<T> Empty = new(Maybe<ChangedValue<T>>.None);

        private readonly Maybe<ChangedValue<T>> _state;

        public bool HasChange => _state.HasValue;

        public static implicit operator OptionalValueChange<T>(ChangedValue<T> source)
        {
            return source == null ? Empty : With(source.Value);
        }

        private OptionalValueChange(Maybe<ChangedValue<T>> state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public static OptionalValueChange<T> With(T value)
        {
            return value
                .AsChangedValue()
                .FromNullable()
                .Transform(m => new OptionalValueChange<T>(m));
        }

        public TOut Match<TOut>(Func<T, TOut> fromChange, Func<TOut> fromEmpty)
        {
            if (fromChange == null) throw new ArgumentNullException(nameof(fromChange));
            if (fromEmpty == null) throw new ArgumentNullException(nameof(fromEmpty));

            return _state.Match(change => fromChange(change.Value), fromEmpty);
        }
    }
}
