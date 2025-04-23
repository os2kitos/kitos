using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PubSub.Application.Mapping;
using PubSub.Application.Services;
using PubSub.Core.Config;
using PubSub.Core.Services.CallbackAuthentication;
using PubSub.Core.Services.CallbackAuthenticator;
using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;
using PubSub.DataAccess;
using PubSub.DataAccess.Repositories;
using RabbitMQ.Client;

namespace PubSub.Application.Config;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, ConfigurationManager configuration)
    {
        var connectionFactory = GetConnectionFactory(configuration);
        services.AddSingleton<IConnectionFactory>(_ => connectionFactory);
        services.AddScoped<ITopicConsumerInstantiatorService, RabbitMQTopicConsumerInstantiatorService>();
        services.AddSingleton<IConnectionManager, RabbitMQConnectionManager>();
        services.AddScoped<IPublisherService, RabbitMQPublisherService>();

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
        return services;
    }

    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddDbContext<PubSubContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });
        return services;
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Custom signature validator; consider refactoring if needed
                    SignatureValidator = (token, _) => TokenValidator.ValidateTokenAsync(token, configuration, services).GetAwaiter().GetResult(),
                    ValidateIssuerSigningKey = false,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
                options.IncludeErrorDetails = true;
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Constants.Config.Validation.CanPublishPolicy, policy =>
                policy.RequireClaim("CanPublish", "true"));
        });

        return services;
    }

    public static IServiceCollection AddPubSubServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddRabbitMQ(configuration);
        services.AddHttpClient<ISubscriberNotifierService, HttpSubscriberNotifierService>();
        services.AddTransient<IPayloadSerializer, JsonPayloadSerializer>();
        services.AddScoped<ISubscriberNotifierService, HttpSubscriberNotifierService>();
        services.AddSingleton<ITopicConsumerStore, InMemoryTopicConsumerStore>();
        services.AddScoped<IRabbitMQConsumerFactory, RabbitMQConsumerFactory>();

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ISubscriptionMapper, SubscriptionMapper>();

        services.AddTransient<ICallbackAuthenticatorConfig>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var pubSubApiKey = configuration.GetValue<string>(Constants.Config.CallbackAuthentication.PubSubApiKey)
                               ?? throw new ArgumentNullException("No API key for callback authentication found in appsettings");

            return new CallbackAuthenticatorConfig() { ApiKey = pubSubApiKey };
        });
        services.AddTransient<ICallbackAuthenticator, HmacCallbackAuthenticator>();

        services.AddRequestMapping();

        return services;
    }
}
