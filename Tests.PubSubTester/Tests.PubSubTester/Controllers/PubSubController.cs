using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Tests.PubSubTester.DTOs;

namespace Tests.PubSubTester.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PubSubController(ILogger<PubSubController> logger) : ControllerBase
    {
        //Select an api url here depending on if you are connecting to a local PubSub api or the one on the staging log server
        //private static readonly string PubSubApiUrl = "http://10.212.74.11:8080";
        private static readonly string PubSubApiUrl = "http://localhost:8080";

        [HttpPost]
        [Route("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequestWithTokenDTO request)
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {request.Token}");
            var content = new StringContent(JsonConvert.SerializeObject(request.Subscription), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/subscribe", content);

            return Ok(response);
        }

        [HttpPost]
        [Route("callback/{id}")]
        public IActionResult Callback(string id, [FromBody] string message)
        {
            logger.LogInformation($"callbackId: {id}, message: {message}");

            using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes("local-no-secret"));
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(message));
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

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(PubSubApiUrl);
            return client;
        }

        [HttpPost]
        [Route("publish")]
        public async Task<IActionResult> Publish(PublishRequestDTO request)
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {request.Token}");
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/publish", content);
            return Ok(response);
        }
    }
}
