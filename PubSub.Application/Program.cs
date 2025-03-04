using Microsoft.IdentityModel.Tokens;
using PubSub.Application;
using PubSub.Application.DTOs;
using PubSub.Core.Consumers;
using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;
using PubSub.Core.Services.Subscribe;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
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
            SignatureValidator = (token, _) => 
            {
                var validatedToken = ValidateTokenAsync(token, builder.Configuration).GetAwaiter().GetResult();
                return validatedToken;
            },
            ValidateIssuerSigningKey = false,
            ValidateIssuer = false,
            ValidateAudience = false,
        };
        options.IncludeErrorDetails = true;
    }); 

await builder.Services.AddRabbitMQ(builder.Configuration);
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

static async Task<SecurityToken> ValidateTokenAsync(string token, IConfiguration configuration)
{
    var httpClient = new HttpClient();

    var jwtValidationApiUrl = configuration["JwtValidation:ApiUrl"];
    if (jwtValidationApiUrl == null)
        throw new ArgumentNullException("JwtValidation:ApiUrl");

    var validationEndpoint = configuration["JwtValidation:ValidationEndpoint"];
    if (validationEndpoint == null)
        throw new ArgumentNullException("JwtValidation:ValidationEndpoint");

    var request = new HttpRequestMessage(HttpMethod.Post, $"{jwtValidationApiUrl}{validationEndpoint}");
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    request.Content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("token", token)
    });

    var response = await httpClient.SendAsync(request);

    var content = await response.Content.ReadAsStringAsync();
    var tokenResponse = JsonSerializer.Deserialize<TokenIntrospectiveResponseDTO>(content);

    if (tokenResponse == null || !tokenResponse.Active)
    {
        throw new SecurityTokenException("Invalid token");
    }

    return new JsonWebToken(token);
}