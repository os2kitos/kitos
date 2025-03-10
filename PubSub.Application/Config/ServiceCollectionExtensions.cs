using PubSub.Application.Mapping;
using PubSub.Core.Managers;
using PubSub.Core.Services.Publisher;
using PubSub.Core.Services.Subscribe;
using RabbitMQ.Client;

namespace PubSub.Application.Config;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, ConfigurationManager configuration)
    {
        var connectionFactory = GetConnectionFactory(configuration);
        services.AddSingleton<IConnectionFactory>(_ => connectionFactory);
        services.AddSingleton<ISubscriberService, RabbitMQSubscriberService>();
        services.AddSingleton<IConnectionManager, RabbitMQConnectionManager>();
        services.AddSingleton<IPublisherService, RabbitMQPublisherService>();

        return services;
    }

    private static IConnectionFactory GetConnectionFactory(IConfiguration configuration)
    {
        var rabbitMQAppSettings = configuration.GetSection(Constants.Config.MessageBus.ConfigSection);
        var hostName = rabbitMQAppSettings.GetValue<string>(Constants.Config.MessageBus.HostName) ?? throw new ArgumentNullException("No RabbitMQ host name found in settings.");
        var user = configuration.GetValue<string>(Constants.Config.MessageBus.User) ?? throw new ArgumentNullException("No RabbitMQ username found in settings.");
        var password = configuration.GetValue<string>(Constants.Config.MessageBus.Password) ?? throw new ArgumentNullException("No RabbitMQ password found in settings.");
        return new ConnectionFactory { HostName = hostName, UserName = user, Password = password };
    }

    public static IServiceCollection AddRequestMapping(this IServiceCollection services)
    {
        services.AddScoped<IPublishRequestMapper, PublishRequestMapper>();
        services.AddScoped<ISubscribeRequestMapper, SubscribeRequestMapper>();
        return services;
    }
}
