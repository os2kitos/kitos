using Core.DomainModel;
using Core.DomainServices.Context;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Presentation.Web.Infrastructure.DI
{
    /// <summary>
    /// Used when there is no active HTTP request (e.g. Hangfire background jobs).
    /// Creates a short-lived DI scope each time Resolve() is called so that
    /// the underlying KitosContext is properly disposed after the query.
    /// </summary>
    public sealed class BackgroundJobFallbackUserResolver : IFallbackUserResolver
    {
        private readonly IServiceProvider _rootProvider;

        public BackgroundJobFallbackUserResolver(IServiceProvider rootProvider)
        {
            _rootProvider = rootProvider;
        }

        public User Resolve()
        {
            using var scope = _rootProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IFallbackUserResolver>().Resolve();
        }
    }
}
