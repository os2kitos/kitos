using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Tests.PubSubTester.DTOs;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Tests.PubSubTester.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PubSubController(ILogger<PubSubController> logger) : ControllerBase
    {
        //Select an api url here depending on if you are connecting to a local PubSub api or the one on the staging log server
        //private static readonly string PubSubApiUrl = "http://10.212.74.11:8080";
        private static readonly string PubSubTesterBaseUrl = "https://localhost:7118/";
        private static readonly string PubSubApiUrl = "https://localhost/";
        private static readonly string KitosApiUrl = "https://localhost:44300/";

        [HttpPost]
        [Route("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequestWithTokenDTO request)
        {
            var client = CreateClient(PubSubApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {request.Token}");
            var content = new StringContent(JsonConvert.SerializeObject(request.Subscription), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/subscription", content);

            return Ok(response);
        }

        [HttpPost]
        [Route("callback/{id}")]
        public IActionResult Callback(string id, [FromBody] MessageDTO<object> message)
        {
            logger.LogInformation($"callbackId: {id}, message: {message}");

            using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes("local-no-secret"));
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)));
            var result = Convert.ToBase64String(hash);

            if (Request.Headers.TryGetValue("Signature-Header", out var signatureHeader))
            {
                logger.LogInformation($"Signature-Header: {signatureHeader}");
            }
            else
            {
                logger.LogWarning("Signature-Header not found in the request.");
            }

            if (signatureHeader == result)
            {
                logger.LogInformation("Signature-Header matches the hash result.");
            }

            return Ok();
        }

        [HttpPost]
        [Route("subscribeToSystemChanges")]
        public async Task<IActionResult> SubscribeToSystemChange()
        {
            var token = await GetKitosToken();
            var request = new SubscribeRequestWithTokenDTO
            {
                Token = token,
                Subscription = new SubscriptionDTO
                {
                    Callback = new Uri(new Uri(PubSubTesterBaseUrl), "api/PubSub/systemChange"),
                    Topics = new List<string> { "KitosITSystemChangedEvent" }

                }
            };
            return await Subscribe(request);
        }

        [HttpPost]
        [Route("subscribeToNonExistantUrl")]
        public async Task<IActionResult> SubscribeToNonExistantUrl()
        {
            var token = await GetKitosToken();
            var request = new SubscribeRequestWithTokenDTO
            {
                Token = token,
                Subscription = new SubscriptionDTO
                {
                    Callback = new Uri(new Uri(PubSubTesterBaseUrl), "api/PubSub/NoUrlHere"),
                    Topics = new List<string> { "KitosITSystemChangedEvent" }

                }
            };
            return await Subscribe(request);
        }

        [HttpPost]
        [Route("deleteSystemChangeSubscription")]
        public async Task<IActionResult> DeleteSubscription(Guid uuid)
        {
            var token = await GetKitosToken();
            var client = CreateClient(PubSubApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var route = $"api/subscription/{uuid}";
            var response = await client.DeleteAsync(route);
            return Ok(response);
        }

        [HttpGet]
        [Route("getAll")]
        public async Task<IActionResult> GetSubscriptions()
        {
            var token = await GetKitosToken();
            var client = CreateClient(PubSubApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            const string route = "api/subscription";
            var response = await client.GetAsync(route);
            var stringResponse = response.Content.ReadFromJsonAsync<IEnumerable<dynamic>>();
            return Ok(stringResponse);
        }

        [HttpPost]
        [Route("systemChange")]
        public IActionResult Callback([FromBody] MessageDTO<SystemChangeDTO> dto)
        {
            return Ok();
        }

        private static async Task<string> GetKitosToken()
        {
            var kitosClient = CreateClient(KitosApiUrl);
            var body = new LoginDTO { Email = "local-api-system-integrator-user@kitos.dk", Password = "localNoSecret" };
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var tokenResponse = await kitosClient.PostAsync("api/authorize/GetToken", content);
            var jsonResponse = await tokenResponse.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            string token = result.response.token; 
            return token;
        }

        private static HttpClient CreateClient(string baseUrl)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            return client;
        }

        [HttpPost]
        [Route("publish")]
        public async Task<IActionResult> Publish(PublishRequestDTO request)
        {
            var client = CreateClient(PubSubApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {request.Token}");
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/publish", content);
            return Ok(response);
        }
    }
}

public class LoginDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class MessageDTO<T>
{
    public T Payload { get; set; }
}

public class SystemChangeDTO
{
    public Guid SystemUuid { get; set; }
    public string? SystemName { get; set; }
    public Guid? DataProcessorUuid { get; set; }
    public string? DataProcessorName { get; set; }
}
