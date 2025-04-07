using Microsoft.IdentityModel.Tokens;
using PubSub.Application;
using PubSub.Core.Consumers;
using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;
using PubSub.Core.Services.Subscribe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PubSub.Application.Config;
using PubSub.Core.Services.CallbackAuthentication;
using PubSub.Core.Services.CallbackAuthenticator;
using PubSub.Core.Config;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable(Constants.Config.Environment.CurrentEnvironment) ?? Constants.Config.Environment.Production;
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json")
    .AddEnvironmentVariables();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(443, listenOptions =>
    {
        var certPassword = Environment.GetEnvironmentVariable(Constants.Config.Certificate.CertPassword);
        listenOptions.UseHttps("/etc/ssl/certs/kitos-pubsub.pfx", certPassword);
    });
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            SignatureValidator = (token, _) => TokenValidator.ValidateTokenAsync(token, builder.Configuration, builder.Services).GetAwaiter().GetResult(),
            ValidateIssuerSigningKey = false,
            ValidateIssuer = false,
            ValidateAudience = false,
        };
        options.IncludeErrorDetails = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.Config.Validation.CanPublishPolicy, policy =>
        policy.RequireClaim("CanPublish", "true"));
});


var pubSubApiKey = builder.Configuration.GetValue<string>(Constants.Config.CallbackAuthentication.PubSubApiKey) ?? throw new ArgumentNullException("No api key for callback authentication found in appsettings");
var callbackAuthenticatorConfig = new CallbackAuthenticatorConfig() { ApiKey = pubSubApiKey };

builder.Services.AddRabbitMQ(builder.Configuration);
builder.Services.AddHttpClient<ISubscriberNotifierService, HttpSubscriberNotifierService>();
builder.Services.AddSingleton<IPayloadSerializer, JsonPayloadSerializer>();
builder.Services.AddSingleton<ISubscriberNotifierService, HttpSubscriberNotifierService>();
builder.Services.AddSingleton<ISubscriptionStore, InMemorySubscriptionStore>();
builder.Services.AddSingleton<IRabbitMQConsumerFactory, RabbitMQConsumerFactory>();
builder.Services.AddSingleton<ICallbackAuthenticatorConfig>(callbackAuthenticatorConfig);
builder.Services.AddSingleton<ICallbackAuthenticator, HmacCallbackAuthenticator>();
builder.Services.AddRequestMapping();


var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
