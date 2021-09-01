using System;
using Core.ApplicationServices.Extensions;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.Shared
{
    /// <summary>
    /// Declares an optional change to a value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OptionalValueChange<T>
    {
        public static readonly OptionalValueChange<T> None = new(Maybe<ChangedValue<T>>.None);

        private readonly Maybe<ChangedValue<T>> _state;

        public bool IsUnchanged => !HasChange;
        public bool HasChange => _state.HasValue;

        public T NewValue => _state.Value.Value;

        public static implicit operator OptionalValueChange<T>(ChangedValue<T> source)
        {
            return source == null ? None : With(source.Value);
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

        public TOut Match<TOut>(Func<T, TOut> fromChange, Func<TOut> fromNone)
        {
            if (fromChange == null) throw new ArgumentNullException(nameof(fromChange));
            if (fromNone == null) throw new ArgumentNullException(nameof(fromNone));

            return _state.Match(change => fromChange(change.Value), fromNone);
        }
    }
}
