using PubSub.Core.Models;

namespace PubSub.Core.Services.Publisher
{
    public interface IPublisherService
    {
        Task Publish(Publication publication);
    }
}
