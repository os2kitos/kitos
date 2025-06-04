using CSharpFunctionalExtensions;

namespace PubSub.Core.Abstractions.MonadExtensions;

public static class MaybeExtensions
{
    public static Maybe<T> Tap<T>(this Maybe<T> maybe, Action<T> action)
    {
        if (maybe.HasValue)
            action(maybe.Value);
        return maybe;
    }
}