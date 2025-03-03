namespace PubSub.Core.Services.Notifier
{
    public interface ISubscriberNotifierService
    {
        Task Notify(string message, string recipient);
    }
}
