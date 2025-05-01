namespace PubSub.Core.DomainModel.Consumer
{
    public interface IConsumer : IDisposable
    {
        Task StartListeningAsync();
    }
}
