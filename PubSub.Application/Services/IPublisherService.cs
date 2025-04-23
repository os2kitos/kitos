using PubSub.Core.Models;

namespace PubSub.Application.Services
{
    public interface IPublisherService
    {
        Task PublishAsync(Publication publication);
    }
}
