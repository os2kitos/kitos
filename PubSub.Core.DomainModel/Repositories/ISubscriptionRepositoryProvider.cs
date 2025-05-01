namespace PubSub.Core.DomainModel.Repositories
{
    public interface ISubscriptionRepositoryProvider
    {
        ISubscriptionRepository Get();
    }
}
