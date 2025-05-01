namespace PubSub.Core.DomainServices
{
    public interface ITopicConsumerInstantiatorService
    {
        Task InstantiateTopic(string topic);
    }
}
