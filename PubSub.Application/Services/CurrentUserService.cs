using System.Security.Claims;
using CSharpFunctionalExtensions;

namespace PubSub.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Maybe<string> UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var id = user?.Identity?.Name;
            return Maybe<string>.From(id);
        }
    }
}