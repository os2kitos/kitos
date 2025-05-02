using PubSub.Core.DomainModel.Serializer;
using PubSub.Application.Services.RabbitMQUtils;
using PubSub.Core.DomainModel.Notifier;
using PubSub.Core.DomainServices;
using Microsoft.Extensions.DependencyInjection;

namespace PubSub.Infrastructure.MessageQueue.Consumer
{
    public class RabbitMQTopicConsumerInstantiatorService : ITopicConsumerInstantiatorService
    {
        private readonly IRabbitMQConnectionManager _connectionManager;
        private readonly ISubscriberNotifier _subscriberNotifierService;
        private readonly ITopicConsumerStore _topicConsumerStore;
        private readonly IRabbitMQConsumerFactory _consumerFactory;
        private readonly IJsonPayloadSerializer _payloadSerializer;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public RabbitMQTopicConsumerInstantiatorService(IRabbitMQConnectionManager connectionManager, ISubscriberNotifier subscriberNotifierService, ITopicConsumerStore topicConsumerStore, IRabbitMQConsumerFactory consumerFactory, IJsonPayloadSerializer payloadSerializer, IServiceScopeFactory serviceScopeFactory)
        {
            _connectionManager = connectionManager;
            _subscriberNotifierService = subscriberNotifierService;
            _topicConsumerStore = topicConsumerStore;
            _consumerFactory = consumerFactory;
            _payloadSerializer = payloadSerializer;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InstantiateTopic(string topic)
        {
            if (!_topicConsumerStore.HasConsumer(topic))
            {
                await CreateAndStartNewConsumerAsync(topic);
            }
        }

        private async Task CreateAndStartNewConsumerAsync(string topic)
        {
            var consumer = _consumerFactory.Create(_connectionManager, _subscriberNotifierService, _payloadSerializer, topic, serviceScopeFactory);
            _topicConsumerStore.SetConsumerForTopic(topic, consumer);
            await consumer.StartListeningAsync();
        }
    }
}
