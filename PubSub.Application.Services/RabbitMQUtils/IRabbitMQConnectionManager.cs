using RabbitMQ.Client;

namespace PubSub.Application.Services.RabbitMQUtils
{
    public interface IRabbitMQConnectionManager: IDisposable
    {
        Task<IConnection> GetConnectionAsync();
    }
}
