using RabbitMQ.Client;

namespace PubSub.Application.Services
{
    public interface IConnectionManager: IDisposable
    {
        Task<IConnection> GetConnectionAsync();
    }
}
