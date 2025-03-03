using PubSub.Application.Mapping;
using PubSub.Core.Managers;
using PubSub.Core.Services.Publisher;
using PubSub.Core.Services.Subscribe;
using RabbitMQ.Client;

namespace PubSub.Application;

public static class ServiceCollectionExtensions
{
    private static readonly string RabbitMQConfigSection = "RabbitMQ";
    private static readonly string RabbitMQHostNameKey = "HostName";

    public static async Task<IServiceCollection> AddRabbitMQ(this IServiceCollection services, ConfigurationManager configuration)
    {
        var rabbitMQSettings = configuration.GetSection(RabbitMQConfigSection);
        var hostName = rabbitMQSettings.GetValue<string>(RabbitMQHostNameKey);
        if (hostName == null) throw new ArgumentNullException("No RabbitMQ host name found in appsettings.");
        var connectionFactory = new ConnectionFactory { HostName = hostName };

        services.AddSingleton<IConnectionFactory>(_ => connectionFactory);
        services.AddSingleton<ISubscriberService, RabbitMQSubscriberService>();
        services.AddSingleton<IConnectionManager, RabbitMQConnectionManager>();
        services.AddSingleton<IPublisherService, RabbitMQPublisherService>();

        return services;
    }

    public static IServiceCollection AddRequestMapping(this IServiceCollection services)
    {
        services.AddScoped<IPublishRequestMapper, PublishRequestMapper>();
        services.AddScoped<ISubscribeRequestMapper, SubscribeRequestMapper>();
        return services;
    }
}