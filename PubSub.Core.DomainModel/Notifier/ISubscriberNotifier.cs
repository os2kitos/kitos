using System.Text.Json;

namespace PubSub.Core.DomainModel.Notifier
{
    public interface ISubscriberNotifier
    {
        Task Notify(JsonElement payload, string recipient);
    }
}
