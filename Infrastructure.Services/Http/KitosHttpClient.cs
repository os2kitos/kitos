using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace Infrastructure.Services.Http;

public class KitosHttpClient : IKitosHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    public KitosHttpClient(ILogger logger)
    {
        _httpClient = new HttpClient();
        _logger = logger;
    }

    public async Task<HttpResponseMessage> PostAsync(object content, Uri uri, string token)
    {
        var serializedObject = JsonConvert.SerializeObject(content);
        var payload = new StringContent(serializedObject, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = payload
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var debug = await _httpClient.SendAsync(request);
            var toLog = await debug.Content.ReadAsStringAsync();
            _logger.Fatal("Response from httpClient in KitosHttpClient: " + toLog + " with statuscode: " + debug.StatusCode);
        }
        catch (Exception e)
        {
            _logger.Fatal($"Failed to send to {request.RequestUri}, payload: {serializedObject}, exception: {e}");
            throw e;
        }

    }
}