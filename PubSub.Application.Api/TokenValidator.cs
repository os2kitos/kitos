using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PubSub.Application.Api.DTOs.Request;
using PubSub.Application.Api.DTOs.Response;

namespace PubSub.Application.Api
{
    public class TokenValidator
    {
        public static async Task<SecurityToken> ValidateTokenAsync(string token, IConfiguration configuration, IServiceCollection services)
        {
            var httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();

            var request = SetupRequest(token, configuration);
            using var response = await httpClient.SendAsync(request);
            await ValidateTokenResponse(response);

            return new JsonWebToken(token);
          }

        private static HttpRequestMessage GetHttpRequestMessage(IConfiguration configuration)
        {
            var jwtValidationApiUrl = configuration[Constants.Config.Validation.Url];
            if (jwtValidationApiUrl == null)
                throw new ArgumentNullException(Constants.Config.Validation.Url);

            var validationEndpoint = configuration[Constants.Config.Validation.Endpoint];
            if (validationEndpoint == null)
                throw new ArgumentNullException(Constants.Config.Validation.Endpoint);

            return new HttpRequestMessage(HttpMethod.Post, $"{jwtValidationApiUrl}{validationEndpoint}");
        }

        private static HttpRequestMessage SetupRequest(string token, IConfiguration configuration)
        {
            var request = GetHttpRequestMessage(configuration);
            const string contentType = "application/json";
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

            var tokenValidationRequest = new TokenValidationRequestDTO { Token = token };
            var jsonContent = JsonSerializer.Serialize(tokenValidationRequest);
            request.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, contentType);

            return request;
        }

        private static async Task ValidateTokenResponse(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenIntrospectiveResponseDTO>(content);

            if (tokenResponse == null || !tokenResponse.Active)
            {
                throw new SecurityTokenException("Invalid token");
            }
        }
    }
}
