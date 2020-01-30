using System;

namespace Core.DomainModel.Result
{
    public sealed class Maybe<T>
    {
        public static readonly Maybe<T> None = new Maybe<T>(false);

        private readonly T _value;

        public bool IsNone => !HasValue;

        public bool HasValue { get; }

        public T Value
        {
            get
            {
                if (HasValue)
                {
                    return _value;
                }
                throw new InvalidOperationException("No item");
            }
        }

        public static implicit operator Maybe<T>(T source)
        {
            return source == null ? None : Some(source);
        }

        private Maybe(bool hasValue, T value = default(T))
        {
            HasValue = hasValue;
            _value = value;
        }

        public static Maybe<T> Some(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new Maybe<T>(true, value);
        }

        public Maybe<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return HasValue ?
                Maybe<TResult>.Some(selector(Value)) :
                Maybe<TResult>.None;
        }

        public T GetValueOrFallback(T fallback)
        {
            if (fallback == null)
                throw new ArgumentNullException(nameof(fallback));

            return HasValue ?
                Value :
                fallback;
        }

        public T GetValueOrDefault()
        {
            return HasValue ?
                Value :
                default(T);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Maybe<T> other))
                return false;

            if (this == None)
            {
                return other == None;
            }

            return Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return HasValue ? Value.GetHashCode() : 0;
        }
    }
}
