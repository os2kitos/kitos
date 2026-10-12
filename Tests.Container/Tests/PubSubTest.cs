using System.Collections.Concurrent;
using System.Text.Json;
using Core.ApplicationServices.KitosEvents;
using CSharpFunctionalExtensions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using PubSub.Core.DomainModel.Notifier;
using PubSub.Core.DomainModel.Publications;
using PubSub.Core.DomainModel.Repositories;
using PubSub.Core.DomainModel.Subscriptions;
using PubSub.Core.DomainModel.Topics;
using PubSub.Infrastructure.MessageQueue;
using PubSub.Infrastructure.MessageQueue.Consumer;
using PubSub.Infrastructure.MessageQueue.Publisher;
using RabbitMQ.Client;

namespace Tests.Container.Tests;

[Collection(MigrationTestsCollection.Name)]
public sealed class PubSubTest : IAsyncLifetime
{
    private const string RabbitMqUserName = "container-test";
    private const string RabbitMqPassword = "container-test-password";

    private readonly IContainer _rabbitMq = new ContainerBuilder("rabbitmq:3.13-alpine")
        .WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUserName)
        .WithEnvironment("RABBITMQ_DEFAULT_PASS", RabbitMqPassword)
        .WithPortBinding(5672, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5672))
        .Build();

    public Task InitializeAsync() => _rabbitMq.StartAsync();

    public Task DisposeAsync() => _rabbitMq.DisposeAsync().AsTask();

    [Fact]
    public async Task PubSub_SendsExpectedMessage_ForEachKitosQueueTopic()
    {
        var messages = new Dictionary<string, string>
        {
            [KitosQueueTopics.SystemChangedEventTopic] =
                """{"SystemUuid":"11111111-1111-1111-1111-111111111111","SystemName":"Changed system"}""",
            [KitosQueueTopics.KitosItSystemUsageCreatedEvent] =
                """{"SystemUuid":"22222222-2222-2222-2222-222222222222","OrganizationUuid":"33333333-3333-3333-3333-333333333333"}"""
        };
        var subscriptions = messages.Keys
            .Select(topic => new Subscription($"callback://{topic}", topic, "container-test"))
            .ToArray();
        var repository = new InMemorySubscriptionRepository(subscriptions);
        var notifier = new RecordingSubscriberNotifier(messages.Count);
        var services = new ServiceCollection()
            .AddSingleton<ISubscriptionRepository>(repository)
            .BuildServiceProvider();
        var connectionManager = new RabbitMQConnectionManager(new ConnectionFactory
        {
            HostName = _rabbitMq.Hostname,
            Port = _rabbitMq.GetMappedPublicPort(5672),
            UserName = RabbitMqUserName,
            Password = RabbitMqPassword
        });
        var serializer = new JsonPayloadSerializer();
        var consumers = messages.Keys
            .Select(topic => new RabbitMQConsumer(
                connectionManager,
                notifier,
                serializer,
                topic,
                services.GetRequiredService<IServiceScopeFactory>()))
            .ToArray();

        try
        {
            foreach (var consumer in consumers)
                await consumer.StartListeningAsync();
            var publisher = new RabbitMQPublisher(connectionManager, serializer);

            foreach (var (topic, payload) in messages)
            {
                using var document = JsonDocument.Parse(payload);
                await publisher.PublishAsync(new Publication(
                    new Topic(topic),
                    document.RootElement.Clone()));
            }

            var received = await notifier.WaitForMessagesAsync(TimeSpan.FromSeconds(10));

            Assert.Equal(messages.Count, received.Count);
            foreach (var (topic, payload) in messages)
            {
                var callback = $"callback://{topic}";
                Assert.True(received.TryGetValue(callback, out var actualPayload));
                Assert.Equal(payload, actualPayload);
            }
        }
        finally
        {
            foreach (var consumer in consumers)
                consumer.Dispose();

            connectionManager.Dispose();
            await services.DisposeAsync();
        }
    }

    private sealed class InMemorySubscriptionRepository(IEnumerable<Subscription> subscriptions)
        : ISubscriptionRepository
    {
        private readonly Subscription[] _subscriptions = subscriptions.ToArray();

        public Task<IEnumerable<Subscription>> GetByTopic(string topic) =>
            Task.FromResult<IEnumerable<Subscription>>(_subscriptions.Where(x => x.Topic == topic));

        public Task<IEnumerable<Subscription>> GetAllByUserId(string userId) =>
            Task.FromResult<IEnumerable<Subscription>>(_subscriptions.Where(x => x.OwnerId == userId));

        public Task<bool> Exists(string topic, string url) =>
            Task.FromResult(_subscriptions.Any(x => x.Topic == topic && x.Callback == url));

        public Task<CSharpFunctionalExtensions.Maybe<Subscription>> GetAsync(Guid uuid) =>
            Task.FromResult(Maybe.From(_subscriptions.FirstOrDefault(x => x.Uuid == uuid)));

        public Task AddAsync(Subscription subscription) => Task.CompletedTask;

        public Task DeleteAsync(Subscription subscription) => Task.CompletedTask;
    }

    private sealed class RecordingSubscriberNotifier(int expectedMessageCount) : ISubscriberNotifier
    {
        private readonly ConcurrentDictionary<string, string> _messages = new();
        private readonly TaskCompletionSource<IReadOnlyDictionary<string, string>> _completion = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Notify(JsonElement payload, string recipient)
        {
            _messages[recipient] = payload.GetRawText();
            if (_messages.Count == expectedMessageCount)
                _completion.TrySetResult(new Dictionary<string, string>(_messages));

            return Task.CompletedTask;
        }

        public async Task<IReadOnlyDictionary<string, string>> WaitForMessagesAsync(TimeSpan timeout)
        {
            var completed = await Task.WhenAny(_completion.Task, Task.Delay(timeout));
            if (completed != _completion.Task)
                throw new TimeoutException("Timed out waiting for PubSub callback messages.");

            return await _completion.Task;
        }
    }
}
