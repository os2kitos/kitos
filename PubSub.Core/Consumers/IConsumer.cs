namespace PubSub.Core.Consumers
{
    public interface IConsumer: IDisposable
    {
        Task StartListeningAsync();
        void AddCallbackUrl(Uri callbackUrl);
    }
}
