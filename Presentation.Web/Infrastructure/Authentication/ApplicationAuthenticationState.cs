using System;
using System.Security.Claims;
using Core.ApplicationServices.Authentication;
using Core.DomainModel;
using Core.DomainModel.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Presentation.Web.Infrastructure.Authentication
{
    public class ApplicationAuthenticationState : IApplicationAuthenticationState
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDomainEvents _domainEvents;

        public ApplicationAuthenticationState(IHttpContextAccessor httpContextAccessor, IDomainEvents domainEvents)
        {
            _httpContextAccessor = httpContextAccessor;
            _domainEvents = domainEvents;
        }

        public void SetAuthenticatedUser(User user, AuthenticationScope scope)
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No active HTTP context available.");

            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, user.Id.ToString()) },
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = scope == AuthenticationScope.Persistent
            };

            httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties).GetAwaiter().GetResult();

            _domainEvents.Raise(new UserLoggedInEvent(user.Uuid));
        }
    }
}
