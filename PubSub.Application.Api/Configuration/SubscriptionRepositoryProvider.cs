using PubSub.Core.DomainModel.Repositories;

namespace PubSub.Application.Api.Configuration
{
    public class SubscriptionRepositoryProvider : ISubscriptionRepositoryProvider
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        public SubscriptionRepositoryProvider(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public ISubscriptionRepository Get()
        {
            using var scope = serviceScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
        }
    }
}
