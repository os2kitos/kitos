using CSharpFunctionalExtensions;

namespace PubSub.Application.Services;

public interface ICurrentUserService
{
    Maybe<string> UserId { get; }
}