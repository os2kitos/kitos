using Microsoft.IdentityModel.Tokens;
using PubSub.Application;
using PubSub.Core.Consumers;
using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;
using PubSub.Core.Services.Subscribe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PubSub.Application.Config;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable(Constants.Config.Environment.CurrentName) ?? Constants.Config.Environment.Production;
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json");

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

builder.Services.AddRabbitMQ(builder.Configuration);
builder.Services.AddHttpClient<ISubscriberNotifierService, HttpSubscriberNotifierService>();
builder.Services.AddSingleton<IMessageSerializer, UTF8MessageSerializer>();
builder.Services.AddSingleton<ISubscriberNotifierService, HttpSubscriberNotifierService>();
builder.Services.AddSingleton<ISubscriptionStore, InMemorySubscriptionStore>();
builder.Services.AddSingleton<IRabbitMQConsumerFactory, RabbitMQConsumerFactory>();
builder.Services.AddRequestMapping();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();