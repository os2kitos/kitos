namespace PubSub.Application.Services
{
    public interface ITopicConsumerInstantiatorService
    {
        Task InstantiateTopic(string topic);
    }
}
