using RabbitMQ.Client;

namespace PubSub.Core.Managers
{
    public interface IConnectionManager: IDisposable
    {
        Task<IConnection> GetConnectionAsync();
    }
}
