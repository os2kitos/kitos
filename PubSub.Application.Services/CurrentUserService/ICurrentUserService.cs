using CSharpFunctionalExtensions;

namespace PubSub.Application.Services.CurrentUserService;

public interface ICurrentUserService
{
    Maybe<string> UserId { get; }
}