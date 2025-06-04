using PubSub.Core.DomainModel.Publications;

namespace PubSub.Core.DomainModel.Publisher
{
    public interface IPublisher
    {
        Task PublishAsync(Publication publication);
    }
}
